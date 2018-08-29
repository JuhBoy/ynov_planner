using System;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using events_planner.Deserializers;
using events_planner.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PlannerApi.Tests.Fixtures;
using PlannerApi.Tests.IntegrationTests.Helpers;

namespace PlannerApi.Tests.IntegrationTests
{
    public partial class BookingControllerTests : IClassFixture<ServerFixtures>
    {
        protected HttpClient HttpClient { get; }
        protected string DbConnection = "server=localhost;port=3306;database=ynov_planner_tests;uid=root;password=root";

        public BookingControllerTests(ServerFixtures server)
        {
            HttpClient = server.Client;
        }

        public partial class Subscriptions : BookingControllerTests
        {
            private PlannerContext Context { get; }

            public Subscriptions(ServerFixtures server) : base(server)
            {
                Context = new PlannerContext((new DbContextOptionsBuilder<PlannerContext>()).UseMySql(DbConnection).Options);
            }

            [Theory, InlineData("DOGSPA-8540", "email@admin.com")]
            public async void ShouldSubscribeToNoWindowEvent(string eventName, string email)
            {
                var @event = Context.Event.FirstOrDefault(e => e.Title == eventName);
                var admin = Context.User.Include(e => e.Role).FirstOrDefault(e => e.Email == email);
                var id = @event.Id;

                string token = TokenBearerHelper.GetTokenFor(admin, Context);
                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenBearerHelper.GetTokenFor(admin, Context));
                HttpResponseMessage body = await HttpClient.GetAsync($"api/booking/subscribe/{id}");
                
                Assert.Equal(HttpStatusCode.NoContent, body.StatusCode);
            }
        }

    }

}