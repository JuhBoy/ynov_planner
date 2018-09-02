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
                Context.Booking.RemoveRange();
                Context.SaveChanges();    
            }
        }

        public class Subscriptions : BookingControllerTests
        {
            public Subscriptions(ServerFixtures server) : base(server) { }

            [Theory, InlineData("DOGSPA-8540", "email@admin.com")]
            public async void ShouldSubscribeToNoWindowEvent(string eventName, string email)
            {
                var @event = Context.Event.FirstOrDefault(e => e.Title == eventName);
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

            [Theory, InlineData("email@admin.com")]
            public async void ShouldValidateWithourModeratorRoles(string email)
            {
                User user = Context.User.Include(e => e.Role).FirstOrDefault(u => u.Email == email);
                Event @event = Context.Event.Find(1);
                
                @event.OpenAt = DateTime.Now.AddDays(-1);
                @event.CloseAt = DateTime.Now.AddDays(1);
                @event.Status = Status.ONGOING;
                var book = new Booking() { UserId = user.Id, EventId = @event.Id, Present = false };
                
                Context.Booking.Add(book);
                Context.Event.Update(@event);
                Context.SaveChanges();
                
                var deserializer = new BookingValidationDeserializer() {
                    EventId = @event.Id,
                    UserId = user.Id
                };
                string json = JsonConvert.SerializeObject(deserializer);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenBearerHelper.GetTokenFor(user, Context));
                HttpResponseMessage body = await HttpClient.PostAsync("api/booking/validate", content);
                
                Assert.Equal("", body.Content.ReadAsStringAsync().Result);
                Assert.Equal(HttpStatusCode.NoContent, body.StatusCode);
            }
        }

    }

}