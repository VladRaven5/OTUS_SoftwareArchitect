using Newtonsoft.Json;

namespace Shared
{
    public class ReceivedMessageArgs
    {
        public string Topic { get; }
        public string Action { get; }
        public string Message { get; }

        public ReceivedMessageArgs(string topic, string action, string message)
        {
            Topic = topic;
            Action = action;
            Message = message;
        }

        public TModel GetModel<TModel>()
        {
            return JsonConvert.DeserializeObject<TModel>(Message);
        }
    }
}