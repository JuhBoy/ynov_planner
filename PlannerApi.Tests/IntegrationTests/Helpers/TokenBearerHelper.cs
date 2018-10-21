using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
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
            Mock<IBookingServices> BookingServices;
            MockUserServices(out Configuration, out PlannerContext, out RoleServices, out PromotionServices, out BookingServices);
            
            IUserServices service = new UserServices(context, Configuration.Object, PromotionServices.Object, 
                RoleServices.Object, BookingServices.Object);
            return service.GenerateToken(ref user);
        }

        public static void MockUserServices(out Mock<IConfiguration> Configuration, 
                                            out Mock<PlannerContext> PlannerContext,
                                            out Mock<IRoleServices> RoleServices,
                                            out Mock<IPromotionServices> PromotionServices,
                                            out Mock<IBookingServices> BookingServices) {
            Configuration = new Mock<IConfiguration>();
            PlannerContext = new Mock<PlannerContext>();
            RoleServices = new Mock<IRoleServices>();
            PromotionServices = new Mock<IPromotionServices>();
            BookingServices = new Mock<IBookingServices>();

            Configuration.Setup(conf => conf["TokenAuthentication:Audience"])
                .Returns("localhost:5000");
            Configuration.Setup(conf => conf["TokenAuthentication:Issuer"])
                .Returns("localhost:5000");
            Configuration.Setup(conf => conf["TokenAuthentication:Secret"])
                .Returns("SECRETE_DEMO_KEY_FOR_DEVELOPMENT_ONLY");
            Configuration.Setup(conf => conf["TokenAuthentication:TokenLifeInHours"])
                .Returns("3");
        }
    }
}