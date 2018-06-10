using events_planner.Services;
using events_planner.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            SetDatabaseContext(services);

            // Add cross origins
            services.AddCors(o => {
                if (Env.IsDevelopment() || Env.IsEnvironment("test"))
                    o.AddPolicy("CrossOrigins", builder => {
                        builder.AllowCredentials();
                        builder.AllowAnyOrigin();
                        builder.AllowAnyMethod();
                        builder.AllowAnyHeader();
                    });
            });

            services.AddMvc();
            services.AddRouting(option => option.LowercaseUrls = true);

            // Enable JWT Authentication
            useJwtAuthentication(services);

            // Add user controller services
            services.AddScoped<IUserServices, UserServices>();
            services.AddScoped<IPromotionServices, PromotionServices>();
            services.AddScoped<IRoleServices, RoleServices>();
            services.AddScoped<IEventServices, EventServices>();
            services.AddScoped<ICategoryServices, CategoryServices>();

            // Email services
            services.AddSingleton<IEmailConfiguration>(Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());
            services.AddTransient<IEmailService, EmailService>();

            // Template generator
            TemplateGenerator generator = new TemplateGenerator(Env) {
                Configuration = Configuration.GetSection("TemplateGenerator").Get<TemplateMailerConfiguration>()
            };
            services.AddSingleton<ITemplateGenerator>(generator);

            services.AddTransient<IImageServices, ImageServices>();

            AddSwagger(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            app.UseCors("CrossOrigins");
            app.UseAuthentication();
            app.UseStaticFiles();
            swaggerConfigure(app);
            app.UseJsonExceptionHandler();
            app.UseMvc();
        }
    }
}
