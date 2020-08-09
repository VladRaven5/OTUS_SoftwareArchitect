using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Shared
{
    public abstract class OutboxMessagesSenderBase : BackgroundService
    {
        protected readonly long _defaulPeriodMsec = 1000;

        public OutboxMessagesSenderBase()
        {
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
        protected abstract Task<bool> HandleOneMessageAsync();
    }
}