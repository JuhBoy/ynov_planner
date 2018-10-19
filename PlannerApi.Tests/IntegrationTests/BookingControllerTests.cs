using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Castle.Core.Internal;
using events_planner.Deserializers;
using Xunit;
using events_planner.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PlannerApi.Tests.Fixtures;
using PlannerApi.Tests.IntegrationTests.Helpers;

namespace PlannerApi.Tests.IntegrationTests
{
    [Collection("Synchrone")] // Integration test must not be run in parallel
    public class BookingControllerTests : IClassFixture<ServerFixtures>, IDisposable
    {
        protected HttpClient HttpClient { get; }
        protected string DbConnection = "server=localhost;port=3306;database=ynov_planner_tests;uid=root;password=root";
        protected PlannerContext Context { get; }

        public BookingControllerTests(ServerFixtures server)
        {
            HttpClient = server.Client;
            Context = new PlannerContext((new DbContextOptionsBuilder<PlannerContext>()).UseMySql(DbConnection).Options);
        }

        public void Dispose()
        {
            var books = Context.Booking.ToArray();
            if (!books.IsNullOrEmpty()) {
                Context.Booking.RemoveRange(books);
            }

            var juryPoints = Context.JuryPoints.ToArray();
            if (!juryPoints.IsNullOrEmpty()) {
                Context.JuryPoints.RemoveRange(juryPoints);
            }
            
            Context.SaveChanges();
        }

        public class Subscriptions : BookingControllerTests
        {
            public Subscriptions(ServerFixtures server) : base(server) { }

            [Theory, InlineData("DOGSPA-8540", "email@admin.com")]
            public async void ShouldSubscribeToNoWindowEvent(string eventName, string email)
            {
                var @event = Context.Event.FirstOrDefault(e => e.Title == eventName);
                @event.OpenAt = DateTime.Now.Add(TimeSpan.FromDays(1));
                @event.CloseAt = DateTime.Now.Add(TimeSpan.FromDays(2));
                Context.Update(@event);
                Context.SaveChanges();
                
                var admin = Context.User.Include(e => e.Role).FirstOrDefault(e => e.Email == email);
                var id = @event.Id;

                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenBearerHelper.GetTokenFor(admin, Context));
                HttpResponseMessage body = await HttpClient.GetAsync($"api/booking/subscribe/{id}");
                
                Assert.Equal("", body.Content.ReadAsStringAsync().Result);
                Assert.Equal(HttpStatusCode.NoContent, body.StatusCode);
            }

            [Theory, InlineData("TALKOLA-8460", "email@admin.com")]
            public async void ShouldNotBeAbleToSubscribeBeforeWindow(string eventName, string email)
            {
                var @event = Context.Event.FirstOrDefault(e => e.Title == eventName);
                var admin = Context.User.Include(e => e.Role).FirstOrDefault(e => e.Email == email);
                var id = @event.Id;

                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenBearerHelper.GetTokenFor(admin, Context));
                HttpResponseMessage body = await HttpClient.GetAsync($"api/booking/subscribe/{id}");
                
                Assert.Equal(HttpStatusCode.BadRequest, body.StatusCode);
            }
        }

        public class Validation : BookingControllerTests
        {
            public Validation(ServerFixtures server) : base(server) { }

            private void CreateFixture(string email, bool present, out Event @event, out User user, out Booking book) {
                user = Context.User.Include(e => e.Role).FirstOrDefault(u => u.Email == email);
                @event = Context.Event.Find(1);
                
                @event.OpenAt = DateTime.Now.AddDays(-1);
                @event.CloseAt = DateTime.Now.AddDays(1);
                @event.Status = Status.ONGOING;
                book = new Booking() { UserId = user.Id, EventId = @event.Id, Present = present };
                
                // Add Fake JuryPoints
                if (present) {
                    @event.JuryPoint = 1345.13F;
                    JuryPoint jp = new JuryPoint() { Points = (float)@event.JuryPoint, UserId = user.Id };
                    Context.JuryPoints.Add(jp);
                }
                JuryPoint jp2 = new JuryPoint() { Points = 1345.14F, UserId = user.Id };
                
                Context.JuryPoints.Add(jp2);
                Context.Booking.Add(book);
                Context.Event.Update(@event);
                Context.SaveChanges();
            }

            private void MakeRequest(BookingValidationDeserializer dsl, User user, out StringContent content) {
                string json = JsonConvert.SerializeObject(dsl);
                content = new StringContent(json, Encoding.UTF8, "application/json");
                
                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    TokenBearerHelper.GetTokenFor(user, Context));
            }

            [Theory, InlineData("email@admin.com", true), InlineData("email@admin.com", false)]
            public async void ShouldValidate_WithoutModeratorRoles(string email, bool presence) {
                CreateFixture(email, false, out var @event, out var user, out var book);
                
                var deserializer = new BookingValidationDeserializer() {
                    EventId = @event.Id,
                    UserId = user.Id,
                    Presence = presence
                };
                MakeRequest(deserializer, user, out var content);
                HttpResponseMessage body = await HttpClient.PostAsync($"api/booking/validate", content);
                var present = Context.Booking.AsNoTracking().FirstOrDefault(p => p.EventId == @event.Id && 
                                                                                 p.UserId == user.Id).Present;
                
                Assert.Equal("", body.Content.ReadAsStringAsync().Result);
                Assert.Equal(HttpStatusCode.NoContent, body.StatusCode);
                Assert.True(present == presence);
            }
            
            [Theory, InlineData("email@admin.com")]
            public async void ShouldInvalidUser_WhenHasBeenSetAsPresent(string email) {
                CreateFixture(email, true, out var @event, out var user, out var book);
                
                var deserializer = new BookingValidationDeserializer() {
                    EventId = @event.Id,
                    UserId = user.Id,
                    Presence = false
                };
                MakeRequest(deserializer, user, out var content);
                HttpResponseMessage body = await HttpClient.PutAsync($"api/booking/change-validation", content);
                
                Assert.Equal("", body.Content.ReadAsStringAsync().Result);
                Assert.Equal(HttpStatusCode.NoContent, body.StatusCode);

                var bookAssertion = Context.Booking.AsNoTracking()
                    .FirstOrDefault(e => e.UserId == user.Id && e.EventId == @event.Id);
                Assert.Equal(false, bookAssertion.Present);
            }
            
            [Theory, InlineData("email@admin.com")]
            public async void ShouldRemoveJuryPoints_WhenUserHasBeenRevoked(string email) {
                CreateFixture(email, true, out var @event, out var user, out var book);
                
                var deserializer = new BookingValidationDeserializer() {
                    EventId = @event.Id,
                    UserId = user.Id,
                    Presence = false
                };
                MakeRequest(deserializer, user, out var content);
                HttpResponseMessage body = await HttpClient.PutAsync($"api/booking/change-validation", content);
                
                Assert.Equal("", body.Content.ReadAsStringAsync().Result);
                Assert.Equal(HttpStatusCode.NoContent, body.StatusCode);

                var bookAssertion = Context.JuryPoints.Where(e => e.UserId == user.Id).ToArray();
                Assert.True(!bookAssertion.Any(a => a.Points.Equals(1345.13F)));
            }
            
            [Theory, InlineData("email@admin.com")]
            public async void ShouldAddJuryPoints_WhenUserHasBeenAdmited(string email) {
                CreateFixture(email, false, out var @event, out var user, out var book);
                
                var deserializer = new BookingValidationDeserializer() {
                    EventId = @event.Id,
                    UserId = user.Id,
                    Presence = true
                };
                MakeRequest(deserializer, user, out var content);
                HttpResponseMessage body = await HttpClient.PutAsync($"api/booking/change-validation", content);
                
                Assert.Equal("", body.Content.ReadAsStringAsync().Result);
                Assert.Equal(HttpStatusCode.NoContent, body.StatusCode);

                var bookAssertion = Context.JuryPoints.Where(e => e.UserId == user.Id).ToArray();
                Assert.True(bookAssertion.Any(a => a.Points.Equals(1345.13F)));
            }
        }

    }

}