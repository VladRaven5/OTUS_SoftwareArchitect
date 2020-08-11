using System;
using Shared;

namespace LabelsService
{
    public class OutboxMessagesSender : OutboxMessagesSenderBase<LabelsRepository>
    {
        public OutboxMessagesSender(IServiceProvider serviceProvider, RabbitMqTopicManager rabbitMq)
            : base(serviceProvider, rabbitMq)
        {

        }
    }
}