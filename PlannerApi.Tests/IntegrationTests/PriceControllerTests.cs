
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using events_planner.Deserializers;
using Xunit;
using events_planner.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PlannerApi.Tests.Fixtures;
using PlannerApi.Tests.IntegrationTests.Helpers;

namespace PlannerApi.Tests.IntegrationTests {
    [Collection("Integration")] // Integration test must not be run in parallel
    public class PriceControllerTests : IClassFixture<ServerFixtures>, IDisposable {

        protected HttpClient HttpClient { get; }
        protected string DbConnection = "server=localhost;port=3306;database=ynov_planner_tests;uid=root;password=root";
        protected PlannerContext Context { get; }

        public PriceControllerTests(ServerFixtures server) {
            HttpClient = server.Client;
            Context = new PlannerContext((new DbContextOptionsBuilder<PlannerContext>()).UseMySql(DbConnection)
                .Options);
        }

        protected StringContent BaseRequest(PriceDeserializer priceDsl, string email) {
            var admin = Context.User.Include(e => e.Role).FirstOrDefault(e => e.Email == email);
                
            var newContent = new StringContent(
                JsonConvert.SerializeObject(priceDsl),
                Encoding.UTF8, 
                "application/json"
            );
                
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                TokenBearerHelper.GetTokenFor(admin, Context));

            return newContent;
        }

        public void Dispose() {
            if (Context.Price.Any()) {
                Context.Price.RemoveRange(Context.Price.ToArray());
                Context.SaveChanges();
            }
        }

        public class Create : PriceControllerTests {
            public Create(ServerFixtures server) : base(server) { }

            [Theory, InlineData("email@admin.com")]
            public async void ShouldNotAuthorizeNegativeValues(string email) {
                int id = Context.Event.First().Id;
                int roleId = Context.Role.First().Id;
                HttpResponseMessage response = await HttpClient.PostAsync("api/price", 
                    BaseRequest(new PriceDeserializer() {Amount = -125, EventId = id, RoleId = roleId}, email));
                
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }

            [Theory, InlineData("email@admin.com")]
            public async void ShouldCreateNewPriceForGivenEvent(string email) {
                int id = Context.Event.First().Id;
                int roleId = Context.Role.First().Id;
                HttpResponseMessage response = await HttpClient.PostAsync("api/price",
                    BaseRequest(new PriceDeserializer() {Amount = 234, EventId = id, RoleId = roleId}, email));
                var prices = Context.Event.Include(p => p.Prices).FirstOrDefault(p => p.Id == 1)?.Prices;
                
                Assert.Empty(await response.Content.ReadAsStringAsync());
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                Assert.NotNull(prices);
                Assert.Single(prices);
            }
        }
    }
}