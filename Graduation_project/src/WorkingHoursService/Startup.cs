using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AutoMapper;
using Shared;

namespace WorkingHoursService
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
            services.AddScoped<RequestsRepository, RequestsRepository>();
            services.AddScoped<WorkingHoursRepository, WorkingHoursRepository>();
            services.AddScoped<ProjectsRepository, ProjectsRepository>();
            services.AddScoped<TasksRepository, TasksRepository>();
            services.AddScoped<UsersRepository, UsersRepository>();
            services.AddScoped<TaskUserWorkingHoursManager, TaskUserWorkingHoursManager>();

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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.ApplicationServices.GetService<BrokerMessagesHandler>();
        }

        private void InitializeRabbitMQ(IServiceCollection services)
        {
            string rabbitMqHost = Configuration.GetValue<string>("RabbitMQ:Host");
            int rabbitMqPort = Configuration.GetValue<int>("RabbitMQ:Port");
            var rabbit = new RabbitMqTopicManager(rabbitMqHost, rabbitMqPort);        
            services.AddSingleton<RabbitMqTopicManager>(rabbit);
            services.AddSingleton<BrokerMessagesHandler, BrokerMessagesHandler>();
        }
    }
}
