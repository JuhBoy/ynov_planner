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
    public partial class UserControllerTests : IClassFixture<ServerFixtures>
    {
        protected HttpClient HttpClient { get; set; }

        public UserControllerTests(ServerFixtures server) {
            HttpClient = server.Client;
        }

        public partial class GetToken : UserControllerTests {

            public GetToken(ServerFixtures server) : base(server) { }
            
            [Theory]
            [InlineData("Student", "123456789")]
            public async Task EnsureTokenIsReturned(string login, string password) {
                string fakeJson = JsonConvert.SerializeObject(new UserConnectionDeserializer() { Login = login, Password = password });

                HttpResponseMessage response = await HttpClient.PostAsync("api/user/token", new StringContent(fakeJson, System.Text.Encoding.UTF8, "application/json"));
                string token = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(token);
            }
        }

        public partial class CreateUser : UserControllerTests {
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
                    UserName = "JhonSmith",
                    RoleName = "Student"
                };

                string json = JsonConvert.SerializeObject(user);

                HttpResponseMessage response = await HttpClient.PostAsync("api/user", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
                string stringResult = await response.Content.ReadAsStringAsync();
                User obj = JsonConvert.DeserializeObject<User>(stringResult);

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.NotNull(obj);
                Assert.Equal(obj.Email, user.Email);
                Assert.Equal(obj.Role.Name, "Student");
            }
        }
    }
}
