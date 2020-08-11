using System;
using Shared;

namespace ProjectsService
{
    public class OutboxMessagesSender : OutboxMessagesSenderBase<ProjectsRepository>
    {
        public OutboxMessagesSender(IServiceProvider serviceProvider, RabbitMqTopicManager rabbitMq)
            : base(serviceProvider, rabbitMq)
        {
        }
    }
}