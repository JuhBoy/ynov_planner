using System;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using events_planner.Models;
using events_planner.Services;
using Microsoft.Extensions.DependencyInjection;

namespace PlannerApi.Tests.UnitTests {
    public class EventsModelTests : IDisposable {
        
        private Mock<EventServices> MockEvService() {
            Mock<PlannerContext> pC = new Mock<PlannerContext>();
            Mock<ICategoryServices> iCs = new Mock<ICategoryServices>();
            
            return new Mock<EventServices>(pC.Object, iCs.Object); 
        }
        
        public void Dispose() {}

        public class DateWindows : EventsModelTests {
            private Event @event { get; }

            public DateWindows()
            {
                @event = new Event() {
                    StartAt = DateTime.Now.AddDays(-5),
                    OpenAt = DateTime.Now.AddHours(1),
                    EndAt = DateTime.Now.AddMinutes(30)
                };
            }

            [Fact]
            public void ShouldBeInOpenSubscriptionWindow() {
                Assert.True(@event.SubscribtionOpen());
            }

            [Fact]
            public void ShouldNotBeInOPenSubscriptionWindow() {
                @event.StartAt = DateTime.Now.AddHours(1);
                Assert.False(@event.SubscribtionOpen());
            }

            [Fact]
            public void ShouldLetUserSubscribeUntilOpenAt() {
                @event.EndAt = null;
                Assert.True(@event.SubscribtionOpen());
            }
            
            [Fact]
            public void ShouldNotLetUserSubscribeAfterOpenAt() {
                @event.OpenAt = DateTime.Now.AddHours(-2);
                @event.EndAt = null;
                Assert.False(@event.SubscribtionOpen());
            }

            [Fact]
            public void ShouldLetSubscribIfNoWindow() {
                @event.EndAt = null;
                @event.StartAt = null;
                Assert.True(@event.SubscribtionOpen());
            }

            [Theory]
            [InlineData("2018-08-30", "2018-09-01", "2018-08-26", "2018-08-28")]
            public void ShouldValidateTimeWindow(string open, string close, string start, string end) {
                var mEvent = @event;
                mEvent.OpenAt = DateTime.Parse(open);
                mEvent.CloseAt = DateTime.Parse(close);
                mEvent.StartAt = DateTime.Parse(start);
                mEvent.EndAt = DateTime.Parse(end);
                
                bool isValid = MockEvService().Object.IsTimeWindowValid(ref mEvent);

                Assert.True(isValid);
            }
            
            [Theory]
            [InlineData("2018-08-30", "2018-08-29", "2018-08-26", "2018-08-28")]
            public void ShouldntValidateTimeWindowWhenCloseBeforeOpen(string open, string close, string start, string end) {
                var mEvent = @event;
                mEvent.OpenAt = DateTime.Parse(open);
                mEvent.CloseAt = DateTime.Parse(close);
                mEvent.StartAt = DateTime.Parse(start);
                mEvent.EndAt = DateTime.Parse(end);
                
                bool isValid = MockEvService().Object.IsTimeWindowValid(ref mEvent);
                
                Assert.False(isValid);
            }
            
            [Theory]
            [InlineData("2018-08-30", "2018-09-01", "2018-08-29", "2018-08-28")]
            public void ShouldntValidateTimeWindowWhenEndBeforeStart(string open, string close, string start, string end) {
                var mEvent = @event;
                mEvent.OpenAt = DateTime.Parse(open);
                mEvent.CloseAt = DateTime.Parse(close);
                mEvent.StartAt = DateTime.Parse(start);
                mEvent.EndAt = DateTime.Parse(end);
                
                bool isValid = MockEvService().Object.IsTimeWindowValid(ref mEvent);
                
                Assert.False(isValid);
            }
        }
    }
}