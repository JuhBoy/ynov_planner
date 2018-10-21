using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using events_planner.Models;
using Microsoft.Extensions.DependencyInjection;
using events_planner.Services;
using PlannerApi.Tests.IntegrationTests.Helpers;

namespace PlannerApi.Tests.UnitTests
{
    public partial class UserServicesTests
    {

        public partial class GetToken : IDisposable
        {
            private Mock<IConfiguration> Configuration;
            private Mock<PlannerContext> PlannerContext;
            private Mock<IRoleServices> RoleServices;
            private Mock<IPromotionServices> PromotionServices;
            private Mock<IBookingServices> BookingServices;

            public GetToken()
            {
                TokenBearerHelper.MockUserServices(out Configuration, out PlannerContext, out RoleServices,
                    out PromotionServices, out BookingServices); 
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
                var us = new Mock<UserServices>(PlannerContext.Object, Configuration.Object, PromotionServices.Object, RoleServices.Object, BookingServices.Object);
                us.Setup(svces => svces.GetRoles(1, "Student")).Returns(new List<Claim>());

                string token = us.Object.GenerateToken(ref user);
                string[] slicedToken = token.Split('.', StringSplitOptions.RemoveEmptyEntries);

                Assert.NotNull(token);
                Assert.Equal(3, slicedToken.Length);
            }

            [Theory, InlineData("myPassword")]
            public void ShouldHashSHA256Password(string password) 
            {
                var service = new Mock<UserServices>(PlannerContext.Object, Configuration.Object, PromotionServices.Object,
                    RoleServices.Object, BookingServices.Object).Object;
                string encodedPWD, encodedPWD2;
                service.GeneratePasswordSha256(password, out encodedPWD);
                service.GeneratePasswordSha256(password, out encodedPWD2);
                
                Assert.NotNull(encodedPWD);
                Assert.NotEmpty(encodedPWD);
                Assert.Equal(encodedPWD, encodedPWD2);
            }
        }
    }
}
