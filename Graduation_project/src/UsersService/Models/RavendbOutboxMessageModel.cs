using System;
using Shared;

namespace UsersService
{
    public class RavendbOutboxMessageModel : BaseModel
    {
        public RavendbOutboxMessageModel()
        {            
        }

        public RavendbOutboxMessageModel(OutboxMessageModel baseModel)
        {
            Init();

            Index = DateTimeOffset.UtcNow.Ticks;
            Message = baseModel.Message;
            Topic = baseModel.Topic;
            Action = baseModel.Action;
        }
        public long Index { get; set; }
        public string Message { get; set; }
        public string Topic { get; set; }        
        public string Action { get; set; }

        public OutboxMessageModel ToBasicOutbox()
        {
            return new OutboxMessageModel
            {
                Id = 0, //doesn't matter
                Message = Message,
                Topic = Topic,
                Action = Action
            };
        }
    }
}