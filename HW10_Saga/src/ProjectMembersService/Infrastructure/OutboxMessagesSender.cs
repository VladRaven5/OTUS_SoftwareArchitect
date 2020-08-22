using System;
using Shared;

namespace ProjectMembersService
{
    public class OutboxMessagesSender : OutboxMessagesSenderBase<ProjectMembersRepository>
    {
        public OutboxMessagesSender(IServiceProvider serviceProvider, RabbitMqTopicManager rabbitMq)
            : base(serviceProvider, rabbitMq)
        {

        }
    }
}