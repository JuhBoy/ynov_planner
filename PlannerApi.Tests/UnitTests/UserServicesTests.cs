using System;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using events_planner.Models;
using Microsoft.Extensions.DependencyInjection;
using events_planner.Services;

namespace PlannerApi.Tests.UnitTests
{
    public partial class UserServicesTests
    {

        public partial class GetToken : IDisposable {

            Mock<IConfiguration> Configuration { get; set; }
            Mock<PlannerContext> PlannerContext { get; set; }
            Mock<IRoleServices> RoleServices { get; set; }
            Mock<IPromotionServices> PromotionServices { get; set; }

            public GetToken() {
                Configuration = new Mock<IConfiguration>();
                PlannerContext = new Mock<PlannerContext>();
                RoleServices = new Mock<IRoleServices>();
                PromotionServices = new Mock<IPromotionServices>();

                Configuration.Setup(conf => conf["TokenAuthentication:Audience"])
                             .Returns("localhost:5000");
                Configuration.Setup(conf => conf["TokenAuthentication:Issuer"])
                             .Returns("localhost:5000");
                Configuration.Setup(conf => conf["TokenAuthentication:Secret"])
                             .Returns("MySecretkeymustbesuperhotstobeacceptedbythehash");
            }

            public void Dispose()
            {
                Configuration = null;
                PlannerContext = null;
            }

            [Fact]
            public void ShouldReturnToken()
            {
                User user = new User() {
                    Id = 1,
                    LastName = "dupon", 
                    FirstName = "George",
                    Password = "My_passwonrd",
                    Email = "email@dqsd.fr",
                    Role = new Role() { Name = "Student" }
                };
                var us = new Mock<UserServices>(PlannerContext.Object, Configuration.Object, PromotionServices.Object, RoleServices.Object);
                us.Setup(svces => svces.GetRoles(1, "Student")).Returns(new List<Claim>());
                
                string token = us.Object.GenerateToken(ref user);
                string[] slicedToken = token.Split('.', StringSplitOptions.RemoveEmptyEntries);

                Assert.NotNull(token);
                Assert.Equal(3, slicedToken.Length);
            }
        }
    }
}
