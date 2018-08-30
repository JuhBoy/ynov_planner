using System;
using System.Reflection;
using events_planner.Models;
using events_planner.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PlannerApi.Tests.IntegrationTests.Helpers;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace PlannerApi.Tests.FunctionalTests
{
    [Collection("Functional")]
    public class UserServicesTests : IDisposable {
        
        protected string DbConnection = "server=localhost;port=3306;database=ynov_planner_tests;uid=root;password=root";
        private PlannerContext Context { get; }
        
        protected Mock<IConfiguration> Cfg;
        protected Mock<PlannerContext> Pc;
        protected Mock<IRoleServices> Irs;
        protected Mock<IPromotionServices> Ips;

        public UserServicesTests() {
            Context = new PlannerContext((new DbContextOptionsBuilder()).UseMySql(DbConnection).Options);
        }

        protected void SetIrsToSuccessReturn() {
            Irs.Setup(e => e.GetForeignerRole()).Returns(new Role() {Name = "Foreigner"});
            Irs.Setup(e => e.GetRoleByName(It.IsAny<string>())).Returns((string e) => {
                if (e.Equals("Student") || e.Equals("Admin") || e.Equals("Moderator")) {
                    return new Role() {Name = e};    
                }
                return null;
            });
        }

        public void Dispose() { }

        public class GetRole : UserServicesTests {
            
            public GetRole(): base() { }

            [Fact]
            public void GetRoleWithNullShouldReturnForeigner()
            {
                TokenBearerHelper.MockUserServices(out Cfg, out Pc, out Irs, out Ips);
                SetIrsToSuccessReturn();
                UserServices service = new Mock<UserServices>(Context, Cfg.Object, Ips.Object, Irs.Object).Object;

                Role role = new Role();
                var args = new Object[] {null, role};
                var m = typeof(UserServices).GetMethod("GetRole", BindingFlags.NonPublic | BindingFlags.Instance);
                
                m.Invoke(service, args);
                role = args[1] as Role;
                
                Assert.NotNull(role);
                Assert.Equal("Foreigner", role.Name);
            }

            [Fact]
            public void GetRoleWithSpecificValueShouldReturnTheRole()
            {
                TokenBearerHelper.MockUserServices(out Cfg, out Pc, out Irs, out Ips);
                SetIrsToSuccessReturn();
                UserServices service = new Mock<UserServices>(Context, Cfg.Object, Ips.Object, Irs.Object).Object;

                Role role = new Role();
                var args = new Object[] {"Student", role};
                var m = typeof(UserServices).GetMethod("GetRole", BindingFlags.Instance | BindingFlags.NonPublic);

                m.Invoke(service, args);
                role = args[1] as Role;

                Assert.NotNull(role);
                Assert.Equal("Student", role.Name);
            }
            
            [Fact]
            public void GetRoleWithwrongValueShouldReturnNull()
            {
                TokenBearerHelper.MockUserServices(out Cfg, out Pc, out Irs, out Ips);
                SetIrsToSuccessReturn();
                UserServices service = new Mock<UserServices>(Context, Cfg.Object, Ips.Object, Irs.Object).Object;

                Role role = new Role();
                var args = new Object[] {"unknown_value", role};
                var m = typeof(UserServices).GetMethod("GetRole", BindingFlags.Instance | BindingFlags.NonPublic);

                m.Invoke(service, args);
                role = args[1] as Role;

                Assert.Null(role);
            }
        }
        
    }
}