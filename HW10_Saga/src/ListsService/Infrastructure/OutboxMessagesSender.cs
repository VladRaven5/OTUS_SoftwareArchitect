using System;
using Shared;

namespace ListsService
{
    public class OutboxMessagesSender : OutboxMessagesSenderBase<ListsRepository>
    {
        public OutboxMessagesSender(IServiceProvider serviceProvider, RabbitMqTopicManager rabbitMq)
            : base(serviceProvider, rabbitMq)
        {
        }
    }
}