using System;
using System.Linq;
using events_planner.Models;
using events_planner.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace PlannerApi.Tests.FunctionalTests {
    [Collection("Synchrone")]
    public class EventServicesTests : IDisposable {

        protected string DbConnection = "server=localhost;port=3306;database=ynov_planner_tests;uid=root;password=root";
        private PlannerContext Context { get; }
        private IEventServices Services { get; }

        public EventServicesTests() {
            Context = new PlannerContext((new DbContextOptionsBuilder()).UseMySql(DbConnection).Options);
            Services = new EventServices(Context, new Mock<ICategoryServices>().Object);
        }

        public virtual void Dispose() { }

        public class Restrictions : EventServicesTests {

            public override void Dispose() {
                if (Context.EventRole.Any()) {
                    Context.EventRole.RemoveRange(Context.EventRole.ToArray());
                    Context.SaveChanges();
                }
            }

            public int[] ConstructEventRoles(out Event @event) {
                int[] roleIds = Context.Role.AsNoTracking().Select(e => e.Id).ToArray();
                @event = Context.Event.First();
                
                foreach (int id in roleIds) {
                    Context.EventRole.Add(new EventRole() {EventId = @event.Id, RoleId = id});
                }
                Context.SaveChanges();
                
                return roleIds;
            }

            [Fact]
            public void ShouldAddAndRemove() {
                int[] roleIds = ConstructEventRoles(out var @event);
                Assert.True(roleIds.Length > 2);
                
                Services.AddAndRemoveEventRoles(new []{ roleIds[0], roleIds[3] }, @event);
                Context.SaveChanges();

                var result = Context.EventRole.Where(e => e.EventId == @event.Id).Select(e => e.RoleId).ToArray();
                
                Assert.Equal(2, result.Length);
                Assert.Contains(roleIds[0], result);
                Assert.Contains(roleIds[3], result);
            }

            [Fact]
            public void ShouldSetEmptyRestrictedRoles() {
                ConstructEventRoles(out var @event);
                
                Services.AddAndRemoveEventRoles(new []{ -5 }, @event);
                Context.SaveChanges();
                var result = Context.EventRole.Where(e => e.EventId == @event.Id).ToArray();
                
                Assert.Equal(0, result.Length);
            }

            [Fact]
            public void ShouldAddRestrictedRoles() {
                int[] roleIds = Context.Role.Select(e => e.Id).ToArray();
                var @event = Context.Event.First();
                
                Services.AddAndRemoveEventRoles(new []{ roleIds[1], roleIds[3] }, @event);
                Context.SaveChanges();

                var result = Context.EventRole.Where(e => e.EventId == @event.Id).Select(e => e.RoleId).ToArray();
                
                Assert.Equal(2, result.Length);
                Assert.Contains(roleIds[1], result);
                Assert.Contains(roleIds[3], result);
            }
        }
    }
}