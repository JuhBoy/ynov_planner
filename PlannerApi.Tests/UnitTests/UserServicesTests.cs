using System;
using System.Linq;
using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using events_planner.Models;
using Microsoft.Extensions.DependencyInjection;
using events_planner.Services;
using Microsoft.EntityFrameworkCore;

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
                var services = new UserServices(PlannerContext.Object, Configuration.Object, PromotionServices.Object, RoleServices.Object);

                User user = new User() { LastName = "dupon", FirstName = "George", Password = "My_passwonrd", Email = "email@dqsd.fr" };
                string token = services.GenerateToken(ref user);
                string[] slicedToken = token.Split('.', StringSplitOptions.RemoveEmptyEntries);

                Assert.NotNull(token);
                Assert.Equal(3, slicedToken.Length);
            }
        }
    }
}
