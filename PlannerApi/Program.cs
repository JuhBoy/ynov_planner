using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using events_planner.Data;
using events_planner.Models;
using Microsoft.Extensions.DependencyInjection;
using NLog.Web;

namespace events_planner
{
    public class Program
    {
        private const string ENV = "Development";

        public static void Main(string[] args)
        {
            // NLog: setup the logger first to catch all errors
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            IWebHost host;
            
            try {
                logger.Debug("Init Server...");
                host = BuildWebHost(args);
            }
            catch (Exception ex) {
                //NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                NLog.LogManager.Shutdown();
                throw;
            }
            
            using (var scope = host.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                try
                {
                    DbSeeder.Initialize(
                        services.GetRequiredService<PlannerContext>(),
                        ENV
                    );
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "An error occurred while seeding the database.");
                }
            }
            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            CreateWebHostbuilder(args).Build();

        public static IWebHostBuilder CreateWebHostbuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseEnvironment(ENV)
                .UseStartup<Startup>()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .UseNLog();

    }
}
