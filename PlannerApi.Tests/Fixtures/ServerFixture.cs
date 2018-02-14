using System;
using System.Net.Http;
using System.IO;
using events_planner;  
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace PlannerApi.Tests.Fixtures {

    public class ServerFixtures : IDisposable {

        public HttpClient Client { get; set; }

        public ServerFixtures() {
            IWebHostBuilder builder = Program.CreateWebHostbuilder(Array.Empty<string>())
                                             .UseContentRoot(Path.GetFullPath("../../../../PlannerApi"));

            TestServer Server = new TestServer(builder);
            Client = Server.CreateClient();
        }

        public void Dispose()
        {
            Client = null; 
        }
    }
}