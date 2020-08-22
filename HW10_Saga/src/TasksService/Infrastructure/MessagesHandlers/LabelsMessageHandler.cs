using System;
using AutoMapper;
using Shared;

namespace TasksService
{
    public class LabelsMessageHandler : DomainMessagesHandlerBase<LabelsRepository>
    {
        protected override string _identifier => "Labels";

        public LabelsMessageHandler(IServiceProvider serviceProvider, IMapper mapper)
            : base(serviceProvider, mapper)
        {
        }

        protected override Type GetMessageTypeForAction(string action)
        {
            switch(action)
            {
                case MessageActions.Created:
                case MessageActions.Updated:
                    return typeof(LabelCreatedUpdatedMessage);
                
                case MessageActions.Deleted:
                    return typeof(LabelDeletedMessage);

                default:
                    throw new NotFoundException($"Action {action} is not known ({_identifier})");                
            }
        }

        protected override void HandleMessageInternal(BaseMessage message, string action, LabelsRepository repository)
        {
            switch(message)
            {
                case LabelCreatedUpdatedMessage createdUpdatedMessage:
                    var model = _mapper.Map<LabelCreatedUpdatedMessage, LabelModel>(createdUpdatedMessage);
                    repository.CreateOrUpdateLabelAsync(model).GetAwaiter().GetResult();
                    break;
                
                case LabelDeletedMessage _:
                    //nothing to do for now
                    break;

                default:
                    ThrowUnknownMessageException(message);
                    break;
            }
        }
    }
}