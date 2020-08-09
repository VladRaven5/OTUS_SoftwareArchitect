using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shared
{
    public abstract class BrokerMessagesHandlerBase : IDisposable
    {
        private readonly int _brokerMessagesLifetimeDays = 2;
        protected readonly RabbitMqTopicManager _rabbitMq;

        public BrokerMessagesHandlerBase(RabbitMqTopicManager rabbitMq, List<TopicQueueBindingArgs> bindingArgs)
        {
            _rabbitMq = rabbitMq;

            InitializeQueues(bindingArgs);

            _rabbitMq.MessageReceived += OnMessageReceived;
        }

        protected virtual void InitializeQueues(List<TopicQueueBindingArgs> bindingArgs)
        {
            _rabbitMq.CreateTopicSubscriptions(bindingArgs);            
        }

        protected abstract void OnMessageReceived(ReceivedMessageArgs messageObject);

        protected virtual void PerformSafetyOnBgThread(Func<Task> action, ReceivedMessageArgs messageObject, TaskScheduler scheduler)
        {
            Task.Run(async() => 
            {
                await action();              
            })
            .ContinueWith((t) =>
            {
                if (t.IsFaulted)
                {
                    HandleMessageFailed(messageObject, t.Exception);
                }                       
                
            }, scheduler);
        }

        protected virtual void HandleMessageFailed(ReceivedMessageArgs messageObject, Exception e)
        {
            throw e;
        }

        protected virtual DateTimeOffset GetRequestIdInvalidationDate()
        {
            return DateTimeOffset.UtcNow.AddDays(_brokerMessagesLifetimeDays);
        }

        public void Dispose()
        {
            _rabbitMq.MessageReceived -= OnMessageReceived;
        }
    }
}