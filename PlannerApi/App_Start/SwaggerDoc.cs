using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Swashbuckle.AspNetCore.Swagger;

namespace events_planner
{
    public partial class Startup
    {
        private void AddSwagger(IServiceCollection services) {
            services.AddSwaggerGen(c => {
               c.SwaggerDoc("v1.0", new Info {
                   Title = "Event planner Ynov",
                   Version = "v1.0"
                });

                var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "events_planner.xml"); 
                c.IncludeXmlComments(xmlPath);
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
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