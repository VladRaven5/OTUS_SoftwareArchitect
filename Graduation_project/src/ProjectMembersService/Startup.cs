using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        }

        private void InitializeRabbitMQ(IServiceCollection services)
        {
            string rabbitMqHost = Configuration.GetValue<string>("RabbitMQ:Host");
            int rabbitMqPort = Configuration.GetValue<int>("RabbitMQ:Port");
            var rabbit = new RabbitMqTopicManager(rabbitMqHost, rabbitMqPort);        
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

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.ApplicationServices.GetService<BrokerMessagesHandler>();
        }
    }
}
