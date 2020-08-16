using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag;
using Shared;

namespace ProjectMembersService
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
            services.AddCors(options => options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            }));

            services.AddControllers();
            services.AddAutoMapper(typeof(Startup));
            services.AddSwaggerDocument();                       

            services.AddSingleton<PostgresConnectionManager, PostgresConnectionManager>();
            services.AddScoped<ProjectMembersRepository, ProjectMembersRepository>();
            services.AddScoped<ProjectsRepository, ProjectsRepository>();
            services.AddScoped<UsersRepository, UsersRepository>();
            services.AddScoped<RequestsRepository, RequestsRepository>();
            services.AddScoped<ProjectMembersManager, ProjectMembersManager>();

            InitializeRabbitMQ(services);
            
            services.AddHostedService<OutboxMessagesSender>();
        }

        private void InitializeRabbitMQ(IServiceCollection services)
        {
            string host = Configuration.GetValue<string>("RabbitMQ:Host");
            int port = Configuration.GetValue<int>("RabbitMQ:Port");
            string username = Configuration.GetValue<string>("RabbitMQ:Username");
            string password = Configuration.GetValue<string>("RabbitMQ:Password"); 

            var rabbit = new RabbitMqTopicManager(host, port, username, password);        
            services.AddSingleton<RabbitMqTopicManager>(rabbit);
            services.AddSingleton<BrokerMessagesHandler, BrokerMessagesHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors();

            app.UseRouting();

            app.UseAuthorization();

            app.UseOpenApi( o =>
            {
                o.PostProcess = (document, request) =>
                {
                    document.Info.Title = "Project members service API";
                    string documentBaseUrl = document.BaseUrl;
                    document.Servers.Clear();
                    document.Servers.Add(new OpenApiServer(){ Description = "Default" , Url = $"{documentBaseUrl}/otusapp/project-members"}); 
                };
            });
            app.UseSwaggerUi3(o => { o.TransformToExternalPath = (s, request) => { return $"/otusapp/project-members/{s}"; }; });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.ApplicationServices.GetService<BrokerMessagesHandler>().Initialize();
        }
    }
}
