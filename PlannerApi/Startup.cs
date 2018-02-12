using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using Microsoft.EntityFrameworkCore;

using events_planner;
using events_planner.Models;
using Swashbuckle.AspNetCore.Swagger;
using events_planner.Services;

namespace events_planner {
    public partial class Startup {

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Env { get; set; }

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            // Mysql Database Context
            services.AddDbContext<PlannerContext>(options => options.UseMySQL(Configuration.GetConnectionString("Mysql")));

            services.AddMvc();
            services.AddRouting(option => option.LowercaseUrls = true);

            // Enable JWT Authentication
            useJwtAuthentication(services);

            // Add user controller services
            services.AddScoped<IUserServices, UserServices>();

            AddSwagger(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            Env = env;

            app.UseAuthentication();
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                // Enable the authentication
                app.UseAuthentication();
            }
            
            swaggerConfigure(app);
            app.UseMvc();
        }
    }
}
