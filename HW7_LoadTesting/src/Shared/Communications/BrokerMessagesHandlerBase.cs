using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shared
{
    public abstract class BrokerMessagesHandlerBase : IDisposable
    {
        protected abstract List<TopicQueueBindingArgs> _bindingArgs { get; }
        private readonly int _brokerMessagesLifetimeDays = 2;
        protected readonly RabbitMqTopicManager _rabbitMq;

        public BrokerMessagesHandlerBase(RabbitMqTopicManager rabbitMq)
        {
            _rabbitMq = rabbitMq;                     
        }

        public virtual void Initialize()
        {
            _rabbitMq.MessageReceived += OnMessageReceived;            
            InitializeQueues(_bindingArgs);   
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