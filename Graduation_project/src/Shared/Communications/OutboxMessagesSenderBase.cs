using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Shared
{
    public abstract class OutboxMessagesSenderBase<TRepository> : BackgroundService where TRepository : IOutboxRepository
    {
        protected readonly long _defaulPeriodMsec = 1000;
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMqTopicManager _rabbitMq;

        protected OutboxMessagesSenderBase(IServiceProvider serviceProvider, RabbitMqTopicManager rabbitMq)
        {
            _serviceProvider = serviceProvider;
            _rabbitMq = rabbitMq;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Stopwatch sw = new Stopwatch();

            while(true)
            {
                try
                {                   
                    if(stoppingToken.IsCancellationRequested)
                        break;
                    
                    sw.Restart();

                    bool isSomeMessagesHandled = await HandleOneMessageAsync();
                    if(isSomeMessagesHandled)
                    {
                        Console.WriteLine($"Outbox message sent");
                    }
                    
                    sw.Stop();
                    long msElapsed = sw.ElapsedMilliseconds;
                    long msToAwait = _defaulPeriodMsec - msElapsed;                    
                    if(!isSomeMessagesHandled && msToAwait > 0)
                    {
                        await Task.Delay((int)msToAwait);
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Exception during sending outbox\n{e.Message}\n{e.StackTrace}");
                }                
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if some data handled</returns>
        protected virtual async Task<bool> HandleOneMessageAsync()
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var outboxRepo = scope.ServiceProvider.GetRequiredService<TRepository>();
                var outboxingMessage = await outboxRepo.PopOutboxMessageAsync();

                if(outboxingMessage == null)
                    return false;
                
                var isSentSuccess = _rabbitMq.SendMessage(outboxingMessage);
                
                if(isSentSuccess)
                {
                    await outboxRepo.DeleteOutboxMessageAsync(outboxingMessage.Id);
                }
                else
                {
                    await outboxRepo.ReturnOutboxMessageToPendingAsync(outboxingMessage.Id);
                }
                
                return true;                
            }
        }
    }
}