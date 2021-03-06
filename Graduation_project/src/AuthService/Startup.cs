using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag;
using Prometheus;
using StackExchange.Redis;

namespace AuthService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerDocument();

            var redisConnectionString = Configuration.GetConnectionString("RedisConnection");
            var redis = ConnectionMultiplexer.Connect(redisConnectionString);
            
            services.AddDataProtection()
                .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
                .SetApplicationName("ProjectMan");
            
            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.Cookie.Name = "UserAuthCookie";
                    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnRedirectToLogin = async (context) => context.Response.StatusCode = 401,
                        OnRedirectToAccessDenied = async (context) => context.Response.StatusCode = 403
                    };
                    options.ExpireTimeSpan = TimeSpan.FromDays(1);
                });


            services.AddSingleton<RavenDBConnectionProvider, RavenDBConnectionProvider>();
            services.AddScoped<Repository, Repository>();
            services.AddScoped<AuthenticationService, AuthenticationService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseHttpMetrics(); //grab common metrics by default

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseOpenApi( o =>
            {
                o.PostProcess = (document, request) =>
                {
                    document.Info.Title = "Authentication service API";
                    string documentBaseUrl = document.BaseUrl;
                    document.Servers.Clear();
                    document.Servers.Add(new OpenApiServer(){ Description = "Default" , Url = $"{documentBaseUrl}/otusapp"}); 
                };
            });
            app.UseSwaggerUi3(o => { o.TransformToExternalPath = (s, request) => { return $"/otusapp/{s}"; }; });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics(); //map metrics to /metrics endpoint
            });
        }
    }
}
