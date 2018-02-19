using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;

using events_planner.Models;
using events_planner.Services;

namespace events_planner {
    public partial class Startup {

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Env { get; set; }

        public Startup(IConfiguration configuration, IHostingEnvironment env) {
            Configuration = configuration;
            Env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            // Mysql Database Context
            if (Env.IsProduction() || Env.IsDevelopment()) {
                services.AddDbContext<PlannerContext>(options => options.UseMySQL(Configuration.GetConnectionString("Mysql")));    
            } else if (Env.IsEnvironment("test")) {
                services.AddDbContext<PlannerContext>(options => options.UseMySQL(Configuration.GetConnectionString("MysqlTests")));    
            }


            services.AddMvc();
            services.AddRouting(option => option.LowercaseUrls = true);

            // Enable JWT Authentication
            useJwtAuthentication(services);

            // Add user controller services
            services.AddScoped<IUserServices, UserServices>();
            services.AddScoped<IPromotionServices, PromotionServices>();
            services.AddScoped<IRoleServices, RoleServices>();

            AddSwagger(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
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
