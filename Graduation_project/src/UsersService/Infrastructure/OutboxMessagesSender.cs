using System;
using Shared;

namespace UsersService
{
    public class OutboxMessagesSender : OutboxMessagesSenderBase<UsersRepository>
    {
        public OutboxMessagesSender(IServiceProvider serviceProvider, RabbitMqTopicManager rabbitMq)
            : base(serviceProvider, rabbitMq)
        {
        }
    }
}