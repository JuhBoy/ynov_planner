using System;
using System.Net.Http;
using System.IO;
using events_planner;  
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace PlannerApi.Tests.Fixtures {

    public class ServerFixtures {

        public static HttpClient Client { get; set; }

        public static HttpClient GetClient() {
            if (Client != null) return Client;

            IWebHostBuilder builder = Program.CreateWebHostbuilder(Array.Empty<string>())
                                 .UseContentRoot(Path.GetFullPath("../../../../PlannerApi"));

            TestServer Server = new TestServer(builder);
            return Client = Server.CreateClient();
        }   
    }
}