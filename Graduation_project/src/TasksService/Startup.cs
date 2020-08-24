using System;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag;
using Prometheus;
using Shared;

namespace TasksService
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
            services.AddScoped<TasksRepository, TasksRepository>();

            services.AddScoped<ProjectsRepository, ProjectsRepository>();
            services.AddScoped<ProjectMembersRepository, ProjectMembersRepository>();
            services.AddScoped<UsersRepository, UsersRepository>();
            services.AddScoped<ListsRepository, ListsRepository>();
            services.AddScoped<LabelsRepository, LabelsRepository>();

            services.AddScoped<RequestsRepository, RequestsRepository>();
            
            InitializeRabbitMQ(services);
            SetRedisCache(services);
            services.AddScoped<TasksManager, TasksManager>();

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
            app.UseHttpMetrics(); //grab common metrics by default

            app.UseAuthorization();

            app.UseOpenApi( o =>
            {
                o.PostProcess = (document, request) =>
                {
                    document.Info.Title = "Tasks service API";
                    string documentBaseUrl = document.BaseUrl;
                    document.Servers.Clear();
                    document.Servers.Add(new OpenApiServer(){ Description = "Default" , Url = $"{documentBaseUrl}/otusapp/tasks"}); 
                };
            });
            app.UseSwaggerUi3(o => { o.TransformToExternalPath = (s, request) => { return $"/otusapp/tasks/{s}"; }; });

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

        private void SetRedisCache(IServiceCollection services)
        {
            var isRedisEnabled = Configuration.GetValue<bool>("Redis:Enabled");

            if(isRedisEnabled)
            {
                Console.WriteLine("Use real Redis cache");
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = Configuration.GetConnectionString("RedisCache");
                    options.InstanceName = "TasksCache";
                });
            }      
            else
            {
                Console.WriteLine("Use fake redis stub");
                services.AddSingleton<IDistributedCache, RedisStub>();
            }     
        }
    }
}
