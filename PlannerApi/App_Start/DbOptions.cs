using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace events_planner
{
    public partial class Startup
    {
        public static readonly LoggerFactory LoggerFactory = new LoggerFactory(new[] {
            new Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider(
                (category, level) => category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information, true
            )
        });

        public void SetDatabaseContext(IServiceCollection services) {
            if (Env.IsDevelopment()) {
                services.AddDbContext<Models.PlannerContext>(optionsBuilder => {
                    optionsBuilder.UseLoggerFactory(LoggerFactory);
                    optionsBuilder.UseMySql(Configuration.GetConnectionString("Mysql"));
                });
            }

            if (Env.IsProduction())
                services.AddDbContext<Models.PlannerContext>(optionsBuilder => optionsBuilder.UseMySql(Configuration.GetConnectionString("Mysql")));

            if (Env.IsEnvironment("test"))
                services.AddDbContext<Models.PlannerContext>(options => options.UseMySql(Configuration.GetConnectionString("MysqlTests")));
        }
    }
}