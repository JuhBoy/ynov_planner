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

        public partial class GetToken : IDisposable {

            Mock<IConfiguration> Configuration { get; set; }
            Mock<PlannerContext> PlannerContext { get; set; }

            public GetToken() {
                Configuration = new Mock<IConfiguration>();
                PlannerContext = new Mock<PlannerContext>();

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
            public async Task ShouldReturnToken()
            {
                var services = new UserServices(PlannerContext.Object, Configuration.Object);

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
