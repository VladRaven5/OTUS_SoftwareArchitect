using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
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

        public RabbitMqTopicManager(string host, int port)
        {            
            var factory = new ConnectionFactory() { HostName = host, Port = port };
            
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(_exchangeName, type:ExchangeType.Topic);        
        }

        public event Action<ReceivedMessageArgs> MessageReceived;

        public void SendMessage(string topic, object message, string action)
        {
            try
            {
                string serializedMessage = JsonConvert.SerializeObject(message);

                IBasicProperties messageProperties = default(IBasicProperties);

                if(!string.IsNullOrWhiteSpace(action))
                {
                    messageProperties = _channel.CreateBasicProperties(); 
                    messageProperties.Headers = new Dictionary<string, object> {{_actionHeaderName, action}};
                } 

                var body = Encoding.UTF8.GetBytes(serializedMessage);

                _channel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: topic,
                    basicProperties: messageProperties, 
                    body: body);               
            }
            catch(Exception exc)
            {
                Console.WriteLine($"{exc.Message}\n{exc.StackTrace}");
            }
        }

        public void CreateTopicSubscriptions(List<(string topic, string queueName)> subscriptionInfo)
        {
            foreach(var (topic, queueName) in subscriptionInfo)
            {
                _channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _channel.QueueBind(
                    queue: queueName,
                    exchange: _exchangeName,
                    routingKey: topic);

                var consumer = new EventingBasicConsumer(_channel);

                consumer.Received += HandleMessage;
                
                _channel.BasicConsume(
                    queue: queueName,
                    autoAck: true,
                    consumer: consumer);

                _consumers.Add(consumer);
            }
        }

        private void HandleMessage(object model, BasicDeliverEventArgs e)
        {
            try
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var headers = e.BasicProperties?.Headers;
                string actionHeader = null;
                
                if(headers.TryGetValue(_actionHeaderName, out object actionHeaderObject))
                {
                    var headerBytes = (byte[])actionHeaderObject;
                    actionHeader = Encoding.UTF8.GetString(headerBytes);
                }
                string topic = e.RoutingKey;

                var args = new ReceivedMessageArgs(topic, actionHeader, message);

                MessageReceived?.Invoke(args);
            }
            catch(Exception exc)
            {
                Console.WriteLine($"{exc.Message}\n{exc.StackTrace}");
            }
            
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