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
            IsInProcess = baseModel.IsInProcess;

            OutboxMessageId = baseModel.GetHashCode();
        }
        public long Index { get; set; }
        public int OutboxMessageId { get; set; }
        public string Message { get; set; }
        public string Topic { get; set; }        
        public string Action { get; set; }
        public bool IsInProcess { get; set; }

        public OutboxMessageModel ToBasicOutbox()
        {
            return new OutboxMessageModel
            {
                Id = OutboxMessageId,
                Message = Message,
                Topic = Topic,
                Action = Action,
                IsInProcess = IsInProcess
            };
        }
    }
}