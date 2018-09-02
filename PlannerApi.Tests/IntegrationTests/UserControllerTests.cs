using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using events_planner.Deserializers;
using events_planner.Models;
using Newtonsoft.Json;
using PlannerApi.Tests.Fixtures;

namespace PlannerApi.Tests.IntegrationTests
{
    [Collection("Synchrone")]
    public class UserControllerTests : IClassFixture<ServerFixtures>
    {
        protected HttpClient HttpClient { get; }

        public UserControllerTests(ServerFixtures server) {
            HttpClient = server.Client;
        }

        public class GetToken : UserControllerTests {

            public GetToken(ServerFixtures server) : base(server) { }
            
            [Theory, InlineData("email@admin.com", "123456789")]
            public async Task EnsureTokenIsReturned(string login, string password) {
                string fakeJson = JsonConvert.SerializeObject(new UserConnectionDeserializer() { Login = login, Password = password });

                HttpResponseMessage response = await HttpClient.PostAsync("api/user/token", new StringContent(fakeJson, System.Text.Encoding.UTF8, "application/json"));
                string token = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(token);
            }
        }

        public class CreateUser : UserControllerTests {
            public CreateUser(ServerFixtures server) : base(server) { }

            [Fact]
            public async Task ShouldCreateUser() {
                UserCreationDeserializer user = new UserCreationDeserializer()
                {
                    FirstName = "Jhon",
                    LastName = "Smith",
                    Email = (DateTimeOffset.Now.ToUnixTimeSeconds().ToString()) + "jhon@mymail.com",
                    Password = "123456789",
                    PhoneNumber = "0619198793",
                    DateOfBirth = DateTime.Today,
                    Location = "Fake_address"
                };

                string json = JsonConvert.SerializeObject(user, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                HttpResponseMessage response = await HttpClient.PostAsync("api/user", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
                string stringResult = await response.Content.ReadAsStringAsync();
                User obj = JsonConvert.DeserializeObject<User>(stringResult);

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.NotNull(obj);
                Assert.Equal(obj.Email, user.Email);
                Assert.Equal("Foreigner", obj.Role.Name);
            }
        }

        public class ReadUser : UserControllerTests {
            public ReadUser(ServerFixtures server) : base(server) { }

            [Theory, InlineData("email@admin.com", "123456789")]
            public async Task ShouldReturnTheUser(string login, string password) {
                string fakeJson = JsonConvert.SerializeObject(new UserConnectionDeserializer() { Login = login, Password = password });
                
                HttpResponseMessage response = await HttpClient.PostAsync("api/user/token", new StringContent(fakeJson, System.Text.Encoding.UTF8, "application/json"));
                string token = await response.Content.ReadAsStringAsync();
                
                Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);

                HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage resp = await HttpClient.GetAsync("api/user", HttpCompletionOption.ResponseContentRead);

                Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            }
        }
    }
}
