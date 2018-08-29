using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration;
using events_planner.Models;
using events_planner.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace PlannerApi.Tests.IntegrationTests.Helpers
{
    public static class TokenBearerHelper
    {
        public static string GetTokenFor(User user, PlannerContext context)
        {
            Mock<IConfiguration> Configuration;
            Mock<PlannerContext > PlannerContext;
            Mock<IRoleServices> RoleServices;
            Mock<IPromotionServices > PromotionServices;
            TokenBearerHelper.MockUserServices(out Configuration, out PlannerContext, out RoleServices, out PromotionServices);
            
            IUserServices service = new UserServices(context, Configuration.Object, PromotionServices.Object, RoleServices.Object);
            return service.GenerateToken(ref user);
        }

        public static void MockUserServices(out Mock<IConfiguration> Configuration, 
                                            out Mock<PlannerContext> PlannerContext,
                                            out Mock<IRoleServices> RoleServices,
                                            out Mock<IPromotionServices> PromotionServices) {
            Configuration = new Mock<IConfiguration>();
            PlannerContext = new Mock<PlannerContext>();
            RoleServices = new Mock<IRoleServices>();
            PromotionServices = new Mock<IPromotionServices>();

            Configuration.Setup(conf => conf["TokenAuthentication:Audience"])
                .Returns("localhost:5000");
            Configuration.Setup(conf => conf["TokenAuthentication:Issuer"])
                .Returns("localhost:5000");
            Configuration.Setup(conf => conf["TokenAuthentication:Secret"])
                .Returns("SECRETE_DEMO_KEY_FOR_DEVELOPMENT_ONLY");
        }
    }
}