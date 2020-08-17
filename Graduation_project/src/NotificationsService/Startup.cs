using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag;
using Prometheus;
using Shared;

namespace NotificationsService
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
            services.AddAutoMapper(typeof(Startup));
            services.AddSwaggerDocument();

            services.AddSingleton<PostgresConnectionManager, PostgresConnectionManager>();

            services.AddScoped<NotificationsRepository, NotificationsRepository>();
            services.AddScoped<ProjectMembersRepository, ProjectMembersRepository>();

            services.AddScoped<RequestsRepository, RequestsRepository>();
            
            InitializeRabbitMQ(services);
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

            app.UseRouting();

            app.UseAuthorization();

            app.UseOpenApi( o =>
            {
                o.PostProcess = (document, request) =>
                {
                    document.Info.Title = "Notifications service API";
                    string documentBaseUrl = document.BaseUrl;
                    document.Servers.Clear();
                    document.Servers.Add(new OpenApiServer(){ Description = "Default" , Url = $"{documentBaseUrl}/otusapp/notifications"}); 
                };
            });
            app.UseSwaggerUi3(o => { o.TransformToExternalPath = (s, request) => { return $"/otusapp/notifications/{s}"; }; });
            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics(); //map metrics to /metrics endpoint
            });

            app.ApplicationServices.GetService<BrokerMessagesHandler>().Initialize();
        }

        private void InitializeRabbitMQ(IServiceCollection services)
        {
            string host = Configuration.GetValue<string>("RabbitMQ:Host");
            int port = Configuration.GetValue<int>("RabbitMQ:Port");
            string username = Configuration.GetValue<string>("RabbitMQ:Username");
            string password = Configuration.GetValue<string>("RabbitMQ:Password");
            services.AddSingleton<RabbitMqTopicManager>(new RabbitMqTopicManager(host, port, username, password));
            services.AddSingleton<BrokerMessagesHandler, BrokerMessagesHandler>();
        }
    }
}
