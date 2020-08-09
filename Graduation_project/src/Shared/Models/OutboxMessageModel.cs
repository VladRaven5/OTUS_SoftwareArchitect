using Newtonsoft.Json;

namespace Shared
{
    public class OutboxMessageModel
    {
        public int Id { get; set; }
        public string Topic { get; set; }
        public string Message { get; set; }
        public string Action { get; set; }

        public static OutboxMessageModel Create(BaseMessage message, string topic, string action)
        {
            return new OutboxMessageModel
            {
                Message = JsonConvert.SerializeObject(message),
                Topic = topic,
                Action = action
            };
        }
    }
}