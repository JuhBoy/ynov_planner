using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using events_planner.Deserializers;
using Newtonsoft.Json.Linq;
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
            [InlineData("Julien", "tototo")]
            public async Task EnsureTokenIsReturned(string login, string password) {
                string fakeJson = JsonConvert.SerializeObject(new UserConnectionDeserializer() { Login = login, Password = password });

                HttpResponseMessage response = await HttpClient.PostAsync("api/user/token", new StringContent(fakeJson, System.Text.Encoding.UTF8, "application/json"));
                JObject content = JObject.Parse(await response.Content.ReadAsStringAsync());
                string token = (string) content.SelectToken("token");

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(token);
            }
        }
    }
}
