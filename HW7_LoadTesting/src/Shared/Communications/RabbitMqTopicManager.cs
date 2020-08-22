using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Shared
{
    public class RabbitMqTopicManager : IDisposable
    {
        private readonly string _exchangeName = "the_exchange";
        private readonly string _actionHeaderName = "action_custom_header";
        private readonly List<EventingBasicConsumer> _consumers = new List<EventingBasicConsumer>();
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqTopicManager(string host, int port, string username, string password)
        {             
            var factory = new ConnectionFactory() { HostName = host, Port = port };     
            if(!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                factory.UserName = username;
                factory.Password = password;
            }       
            
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(_exchangeName, type:ExchangeType.Topic);        
        }

        public event Action<ReceivedMessageArgs> MessageReceived;

        public bool SendMessage(OutboxMessageModel outboxMessage)
        {
            return SendMessage(outboxMessage.Topic, outboxMessage.Message, outboxMessage.Action);
        }

        public bool SendMessage(string topic, string message, string action)
        {
            try
            {
                IBasicProperties messageProperties = _channel.CreateBasicProperties(); 
                messageProperties.Persistent = true;

                if(!string.IsNullOrWhiteSpace(action))
                {
                    messageProperties.Headers = new Dictionary<string, object> {{_actionHeaderName, action}};
                } 

                var body = Encoding.UTF8.GetBytes(message);

                _channel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: topic,
                    basicProperties: messageProperties, 
                    body: body);

                return true;             
            }
            catch(Exception exc)
            {
                Console.WriteLine($"{exc.Message}\n{exc.StackTrace}");
            }
            return false;
        }

        public void CreateTopicSubscriptions(List<TopicQueueBindingArgs> subscriptionInfo)
        {
            foreach(TopicQueueBindingArgs arg in subscriptionInfo)
            {
                _channel.QueueDeclare(
                    queue: arg.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _channel.QueueBind(
                    queue: arg.QueueName,
                    exchange: _exchangeName,
                    routingKey: arg.Topic);

                var consumer = new EventingBasicConsumer(_channel);

                consumer.Received += HandleMessage;
                
                _channel.BasicConsume(
                    queue: arg.QueueName,
                    autoAck: false,
                    consumer: consumer);

                _consumers.Add(consumer);
            }
        }

        private void HandleMessage(object model, BasicDeliverEventArgs e)
        {
            try
            {
                Console.WriteLine($"Message received!");
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var headers = e.BasicProperties?.Headers;
                string actionHeader = GetHeaderValue(headers, _actionHeaderName);                    
                
                string topic = e.RoutingKey;

                var args = new ReceivedMessageArgs(topic, actionHeader, message);

                MessageReceived?.Invoke(args);

                _channel.BasicAck(e.DeliveryTag, false);
                Console.WriteLine($"Message acked!");
            }
            catch(Exception exc)
            {
                Console.WriteLine($"{exc.Message}\n{exc.StackTrace}");
            }
            
        }

        private string GetHeaderValue(IDictionary<string, object> headers, string headerName)
        {
            if(headers.TryGetValue(headerName, out object headerObject))
            {
                var headerBytes = (byte[])headerObject;
                return Encoding.UTF8.GetString(headerBytes);
            }

            return null;
        }

        public void Dispose()
        {
            foreach(var consumer in _consumers)
            {
                consumer.Received -= HandleMessage;
            }            

            _channel.Close();
            _connection.Close();

            _channel.Dispose();
            _connection.Dispose();
        }
    }
}