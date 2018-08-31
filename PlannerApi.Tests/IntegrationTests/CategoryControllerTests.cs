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
    public class CategoryControllerTests : IClassFixture<ServerFixtures> {
        
        protected HttpClient HttpClient { get; }
        protected string DbConnection = "server=localhost;port=3306;database=ynov_planner_tests;uid=root;password=root";
        protected PlannerContext Context { get; }

        public CategoryControllerTests(ServerFixtures server) {
            HttpClient = server.Client;
            Context = new PlannerContext((new DbContextOptionsBuilder<PlannerContext>()).UseMySql(DbConnection)
                .Options);
        }

        public void Dispose() { }

        public class CreateCategory : CategoryControllerTests {
            
            public CreateCategory(ServerFixtures server): base(server) { }

            [Theory, InlineData("email@admin.com")]
            public async void ShouldNotSetNegativeValueForParent(string email) {
                string nameAsGuid = Guid.NewGuid().ToString().Substring(0, 20);
                var categoryDsr = new CategoryDeserializer() {Name = nameAsGuid, ParentCategory = -5};
                var admin = Context.User.Include(e => e.Role).FirstOrDefault(e => e.Email == email);
                
                var newContent = new StringContent(JsonConvert.SerializeObject(categoryDsr),
                                                   Encoding.UTF8, "application/json");
                
                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                                                                     TokenBearerHelper.GetTokenFor(admin, Context));
                HttpResponseMessage response = await HttpClient.PostAsync("api/category", newContent);
                
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.False(Context.Category.Any(c => c.Name.Equals(nameAsGuid)));
            }
            
            [Theory, InlineData("email@admin.com")]
            public async void ShouldNullableValueForParentWhenNotFound(string email) {
                string nameAsGuid = Guid.NewGuid().ToString().Substring(0, 20);
                var categoryDsr = new CategoryDeserializer() {Name = nameAsGuid, ParentCategory = int.MaxValue-1};
                var admin = Context.User.Include(e => e.Role).FirstOrDefault(e => e.Email == email);
                
                var newContent = new StringContent(JsonConvert.SerializeObject(categoryDsr), 
                                                   Encoding.UTF8, "application/json");
                
                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                                                                     TokenBearerHelper.GetTokenFor(admin, Context));
                HttpResponseMessage response = await HttpClient.PostAsync("api/category", newContent);
                
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(Context.Category.Any(c => c.Name.Equals(nameAsGuid)));
            }
        }
    }
}
