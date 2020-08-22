namespace Shared
{
    public class TopicQueueBindingArgs
    {
        public string QueueName { get; }
        public string Topic { get; }

        public TopicQueueBindingArgs(string topic, string queueName)
        {
            Topic = topic;
            QueueName = queueName;
        }
    }
}