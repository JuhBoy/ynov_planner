using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using events_planner.Models;
using events_planner.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace PlannerApi.Tests.FunctionalTests {
    
    [Collection("Synchrone")]
    public class BookingTests : IDisposable {
                
        protected string DbConnection = "server=localhost;port=3306;database=ynov_planner_tests;uid=root;password=root";
        private PlannerContext Context { get; }

        public BookingTests() {
            Context = new PlannerContext((new DbContextOptionsBuilder()).UseMySql(DbConnection).Options);
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
    }
        
        
    }
}