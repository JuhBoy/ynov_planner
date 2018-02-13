using System;
using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using events_planner.Models;
using Microsoft.Extensions.DependencyInjection;

namespace PlannerApi.Tests.UnitTests
{
    public partial class UserServicesTests
    {
        public partial class GetToken {
            [Theory]
            [InlineData("localhost:5000", "localhost:5000", "MySecretkeymustbesuperhotstobeacceptedbythehash")]
            public async Task ShouldReturnToken(string Audience, string Issuer, string Secret)
            {
                var plannerContext = new Mock<PlannerContext>();
                var config = new Mock<IConfiguration>();

                config.Setup(conf => conf["TokenAuthentication:Audience"])
                      .Returns(Audience);
                config.Setup(conf => conf["TokenAuthentication:Issuer"])
                      .Returns(Issuer);
                config.Setup(conf => conf["TokenAuthentication:Secret"])
                      .Returns(Secret);

                var services = new UserServices(plannerContext.Object, config.Object);

                var objectResult = await services.GetToken("myLogin", "MyPassword");
                Type type = objectResult.GetType();

                string token = type.GetProperty("Token").GetValue(objectResult, null) as string;
                string[] slicedToken = token.Split('.', StringSplitOptions.RemoveEmptyEntries);

                Assert.NotNull(token);
                Assert.Equal(3, slicedToken.Length);
            }
        }
    }
}
