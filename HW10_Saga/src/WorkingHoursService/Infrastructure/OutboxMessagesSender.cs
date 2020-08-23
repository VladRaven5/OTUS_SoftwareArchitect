using System;
using Shared;

namespace WorkingHoursService
{
    public class OutboxMessagesSender : OutboxMessagesSenderBase<WorkingHoursRepository>
    {
        public OutboxMessagesSender(IServiceProvider serviceProvider, RabbitMqTopicManager rabbitMq)
            : base(serviceProvider, rabbitMq)
        {

        }
    }
}