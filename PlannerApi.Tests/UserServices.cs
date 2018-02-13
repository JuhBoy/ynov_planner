using System.Threading.Tasks;
using System.Net;
using System;
using System.Net.Http;
using Xunit;
using PlannerApi.Tests.Fixtures;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace PlannerApi.Tests
{
    public class UserServices
    {

        [Fact]
        public async Task DummyTest()
        {
            // Arrange
            HttpClient client = ServerFixtures.GetClient();

            // Act
            HttpResponseMessage response = await client.GetAsync("/api/user");
            string jsonString = await response.Content.ReadAsStringAsync();
            List<string> resultList = JsonConvert.DeserializeObject<List<string>>(jsonString);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(resultList);
            Assert.StrictEqual<int>(2, resultList.Count);
        }
    }
}
