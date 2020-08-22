using System;
using Shared;

namespace TasksService
{
    public class OutboxMessagesSender : OutboxMessagesSenderBase<TasksRepository>
    {
        public OutboxMessagesSender(IServiceProvider serviceProvider, RabbitMqTopicManager rabbitMq)
            : base(serviceProvider, rabbitMq)
        {
        }
    }
}