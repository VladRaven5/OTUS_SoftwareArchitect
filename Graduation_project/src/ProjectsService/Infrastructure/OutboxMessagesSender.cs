using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace ProjectsService
{
    public class OutboxMessagesSender : OutboxMessagesSenderBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMqTopicManager _rabbitMq;

        public OutboxMessagesSender(IServiceProvider serviceProvider, RabbitMqTopicManager rabbitMq)
        {
            _serviceProvider = serviceProvider;
            _rabbitMq = rabbitMq;
        }

        protected override async Task<bool> HandleOneMessageAsync()
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var projectsRepo = scope.ServiceProvider.GetRequiredService<ProjectsRepository>();
                var outboxingMessage = await projectsRepo.PopOutboxMessageAsync();

                if(outboxingMessage == null)
                    return false;
                
                _rabbitMq.SendMessage(outboxingMessage);
                
                return true;                
            }
        }
    }
}