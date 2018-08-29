using System;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using events_planner.Models;
using Microsoft.Extensions.DependencyInjection;

namespace PlannerApi.Tests.UnitTests {
    public partial class EventsModelTests {

        public partial class DateWindows : IDisposable {
            private Event @event { get; }

            public DateWindows()
            {
                @event = new Event() {
                    StartAt = DateTime.Now.AddDays(-5),
                    OpenAt = DateTime.Now.AddHours(1),
                    EndAt = DateTime.Now.AddMinutes(30)
                };
            }

            public void Dispose() {}

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
        }
    }
}