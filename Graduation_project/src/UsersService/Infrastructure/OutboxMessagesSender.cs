using System;
using System.Threading.Tasks;
using Shared;

namespace UsersService
{
    public class OutboxMessagesSender : OutboxMessagesSenderBase<UsersRepository>
    {
        public OutboxMessagesSender(IServiceProvider serviceProvider, RabbitMqTopicManager rabbitMq)
            : base(serviceProvider, rabbitMq)
        {            
        }

        //ravendb is too slow at update entities and marked as processing entities handles several times
        protected override async Task<bool> HandleOneMessageAsync()
        {
            await Task.Delay(200);
            return await base.HandleOneMessageAsync();
        }
    }
}