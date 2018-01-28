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

using events_planner.Models;
using Swashbuckle.AspNetCore.Swagger; 

namespace events_planner {
    public class Startup {

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

            AddSwagger(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            Env = env;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            swaggerConfigure(app);
            app.UseMvc();
        }

        private void AddSwagger(IServiceCollection services) {
            services.AddSwaggerGen(c => {
               c.SwaggerDoc("v1.0", new Info {
                   Title = "Event planner Ynov",
                   Version = "v1.0"
                });

                var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "events_planner.xml"); 
                c.IncludeXmlComments(xmlPath);
            }); 
        }

        private void swaggerConfigure(IApplicationBuilder application) {
            application.UseSwagger();
            application.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Ynov Events Planner v1.0");
            });
        }
    }
}
