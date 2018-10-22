using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using events_planner.Models;
using events_planner.Services;
using events_planner.Constants;
using events_planner.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest.TransientFaultHandling;
using Moq;
using Xunit;

namespace PlannerApi.Tests.FunctionalTests {
    
    [Collection("Synchrone")]
    public class BookingTests : IDisposable {
                
        protected string DbConnection = "server=localhost;port=3306;database=ynov_planner_tests;uid=root;password=root";
        private PlannerContext Context { get; }

        public BookingTests() {
            Context = new PlannerContext((new DbContextOptionsBuilder()).UseMySql(DbConnection).Options);
        }

        public IBookingServices GetBookingService() {
            var bs = new BookingServices(Context, (new Mock<IEmailService>()).Object);
            return bs;
        }

        public void Dispose() {
            if (!Context.Booking.Any()) { }
            Context.RemoveRange(Context.Booking.ToList());
            Context.SaveChanges();
        }

        public class ModelValidation : BookingTests {

            [Theory, InlineData("julien@gmail.com", 5), InlineData("julien@gmail.com", 3), InlineData("julien@gmail.com", 2)]
            public void ShouldComputeBookingParticipation(string email, int tPresent) {
                int count = tPresent;
                var mUser = Context.User.AsNoTracking().FirstOrDefault(u => u.Email.Equals(email));
                IEnumerable<Event> list = Context.Event.ToArray().Take(6);
                foreach (var @event in list) {
                    bool present = (tPresent > 0);
                    var book = new Booking() {EventId = @event.Id, Present = present, UserId = mUser.Id};
                    Context.Booking.Add(book);
                    tPresent--;
                }
                Context.SaveChanges();

                User user = Context.User.Include(i => i.Bookings).FirstOrDefault(i => i.Id.Equals(mUser.Id));
                
                Assert.Equal(count, user.Participations);
                Dispose(); // Reset all booking
            }

            [Theory, InlineData("julien@gmail.com"),InlineData("email@admin.com")]
            public async void ShouldBeAvailable_WhenUserAndEventRespectRules(string email) {
                var @event = Context.Event.FirstOrDefault(e => e.SubscribtionOpen() && e.Forward());
                var user = Context.User.FirstOrDefault(u => u.Email == email);

                var rsp = await GetBookingService().IsBookableAsync(@event, user);
                
                Assert.Null(rsp);
            }
            
            [Fact]
            public async void ShouldNotBeAvailable_WhenUserMissing() {
                var @event = Context.Event.FirstOrDefault(e => e.SubscribtionOpen());

                var rsp = await GetBookingService().IsBookableAsync(@event, null);
                
                Assert.NotNull(rsp);
                Assert.Equal(rsp.Value, ApiErrors.UserNotFound);
            }
            
            [Fact]
            public async void ShouldNotBeAvailable_WhenEventMissing() {
                var user = Context.User.FirstOrDefault(u => u.Email == "julien@gmail.com");

                var rsp = await GetBookingService().IsBookableAsync(null, user);
                
                Assert.NotNull(rsp);
                Assert.Equal(rsp.Value, ApiErrors.EventNotFound);
            }
            
            [Fact]
            public async void ShouldNotBeAvailable_WhenEventExpired() {
                var user = Context.User.FirstOrDefault(u => u.Email == "julien@gmail.com");
                var @event = Context.Event.First();
                @event.CloseAt = DateTime.Now.Add(TimeSpan.FromDays(-1));
                @event.SubscribeNumber = int.MaxValue;
                @event.StartAt =  DateTime.Now.Add(TimeSpan.FromDays(-1));
                @event.EndAt = DateTime.Now.Add(TimeSpan.FromDays(1));
                
                var rsp = await GetBookingService().IsBookableAsync(@event, user);
                
                Assert.NotNull(rsp);
                Assert.Equal(rsp.Value, ApiErrors.EventExpired);
            }
            
            [Fact]
            public async void ShouldNotBeAvailable_WhenUserAlreadyBooked() {
                var user = Context.User.FirstOrDefault(u => u.Email == "julien@gmail.com");
                var @event = Context.Event.First();
                var book = new Booking() { User = user, Event = @event };
                Context.Booking.Add(book);
                Context.SaveChanges();
                
                var rsp = await GetBookingService().IsBookableAsync(@event, user);
                
                Assert.NotNull(rsp);
                Assert.Equal(rsp.Value, ApiErrors.AlreadyBooked);
            }

            [Fact]
            public async void ShouldValidateAndCreateJuryPoints_WhenPresent_ThenDeleteAndUnvalidate() {
                var @event = Context.Event.First();
                @event.JuryPoint = 0.5f;
                var user = Context.User.First();
                var book = new Booking() {
                    UserId = user.Id,
                    EventId = @event.Id,
                    Present = false,
                    Event = @event
                };

                await GetBookingService().SetBookingPresence(book, true);
                var jPoints = Context.JuryPoints.FirstOrDefault(j => j.Points - @event.JuryPoint < 0.01);

                Assert.NotNull(jPoints);
                Assert.True(book.Present);

                await GetBookingService().SetBookingPresence(book, false);
                var nullPoints = Context.JuryPoints.FirstOrDefault(j => j.Points - @event.JuryPoint < 0.01);
                
                Assert.Null(nullPoints);
                Assert.False(book.Present);
            }
        }

        public class Confirmation : BookingTests {

            [Fact]
            public async void ShouldConfirm_WhenEventIsActive() {
                User user = Context.User.First();
                var @ev = new Event() {
                    CloseAt = DateTime.Now.AddDays(1),
                    EndAt = DateTime.Now.AddDays(1),
                    StartAt = DateTime.Now.AddDays(-1),
                    OpenAt = DateTime.Now.AddHours(10),
                    Status = "pending",
                    SubscribeNumber = 5,
                    SubscribedNumber = 0,
                    Description = "fake",
                    Title = "fake",
                    RestrictedEvent = false,
                    JuryPoint = 0.5f,
                    ValidationRequired = false,
                    ValidatedNumber = 0
                };
                var book = new Booking() { UserId = user.Id, Event = @ev, Validated = false };
                Context.Event.Add(@ev);
                Context.Booking.Add(book);
                Context.SaveChanges();

                book.Validated = true;
                await GetBookingService().SetBookingConfirmation(false, book);
                
                Assert.True(book.Validated);
                Assert.True(@ev.ValidatedNumber == 1);
            }
            
            [Fact]
            public async void ShouldUnConfirm_WhenEventIsActive() {
                User user = Context.User.First();
                var @ev = new Event() {
                    CloseAt = DateTime.Now.AddDays(1),
                    EndAt = DateTime.Now.AddDays(1),
                    StartAt = DateTime.Now.AddDays(-1),
                    OpenAt = DateTime.Now.AddHours(10),
                    Status = "pending",
                    SubscribeNumber = 5,
                    SubscribedNumber = 1,
                    Description = "fake",
                    Title = "fake",
                    RestrictedEvent = false,
                    JuryPoint = 0.5f,
                    ValidationRequired = false,
                    ValidatedNumber = 1
                };
                var book = new Booking() { UserId = user.Id, Event = @ev, Validated = true };
                Context.Event.Add(@ev);
                Context.Booking.Add(book);
                Context.SaveChanges();

                book.Validated = false;
                await GetBookingService().SetBookingConfirmation(true, book);
                
                Assert.False(book.Validated);
                Assert.True(@ev.ValidatedNumber == 0);
                Assert.True(@ev.SubscribedNumber == 0);
            }
        }

        public class Subscriptions : BookingTests {

            [Fact]
            public async void ShouldSubscribeUser_WhenEventIsFree() {
                var @event = Context.Event.FirstOrDefault(e => e.Forward() && e.SubscribtionOpen());
                @event.ValidatedNumber = 0;
                @event.SubscribedNumber = 0;
                var user = Context.User.First();

                await GetBookingService().SubscribeUserToEvent(@event, user);
                var book = Context.Booking.FirstOrDefault(b => b.UserId == user.Id && b.EventId == @event.Id);
                
                Assert.NotNull(book);
                Assert.True(@event.ValidatedNumber == 1);
                Assert.True(@event.SubscribedNumber == 1);
                Assert.True(book.Validated);
            }
        }
    }
}