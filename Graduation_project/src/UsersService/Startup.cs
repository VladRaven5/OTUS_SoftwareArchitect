using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared;

namespace UsersService
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

            InitializeRabbitMQ(services);
            
            services.AddSingleton<DBConnectionProvider, DBConnectionProvider>();
            services.AddScoped<Repository, Repository>();
            services.AddScoped<UsersManager, UsersManager>();
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
        }

        private void InitializeRabbitMQ(IServiceCollection services)
        {
            string rabbitMqHost = Configuration.GetValue<string>("RabbitMQ:Host");
            int rabbitMqPort = Configuration.GetValue<int>("RabbitMQ:Port");
            services.AddSingleton<RabbitMqTopicManager>(new RabbitMqTopicManager(rabbitMqHost, rabbitMqPort));
        }
    }
}
