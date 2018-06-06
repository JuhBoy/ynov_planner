using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace events_planner
{
    public partial class Startup
    {
        public void useJwtAuthentication(IServiceCollection services) {
            SymmetricSecurityKey symmertricKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Configuration["TokenAuthentication:Secret"])
            );

            services.AddAuthentication(options => {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options => {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters() {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = symmertricKey,

                    RoleClaimType = "pk_user",

                    ValidateIssuer = true,
                    ValidIssuer = Configuration["TokenAuthentication:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = Configuration["TokenAuthentication:Audience"],

                    ValidateLifetime = true
                };
            });
        }
    }
}