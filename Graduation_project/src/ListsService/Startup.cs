using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared;

namespace ListsService
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

            services.AddSingleton<PostgresConnectionManager, PostgresConnectionManager>();
            services.AddScoped<ListsRepository, ListsRepository>();
            services.AddScoped<ProjectsRepository, ProjectsRepository>();
            services.AddScoped<RequestsRepository, RequestsRepository>();
            
            InitializeRabbitMQ(services);
            services.AddScoped<ListsManager, ListsManager>();

            services.AddHostedService<OutboxMessagesSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.ApplicationServices.GetService<BrokerMessagesHandler>();
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
