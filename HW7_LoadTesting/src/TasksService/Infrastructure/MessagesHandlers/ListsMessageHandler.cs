using System;
using AutoMapper;
using Shared;

namespace TasksService
{
    public class ListsMessageHandler : DomainMessagesHandlerBase<ListsRepository>
    {
        protected override string _identifier => "Lists";

        public ListsMessageHandler(IServiceProvider serviceProvider, IMapper mapper)
            : base(serviceProvider, mapper)
        {
        }

        protected override Type GetMessageTypeForAction(string action)
        {
            switch(action)
            {
                case MessageActions.Created:
                    return typeof(ListCreatedMessage);

                case MessageActions.Updated:
                    return typeof(ListUpdatedMessage);
                
                case MessageActions.Deleted:
                    return typeof(ListDeletedMessage);

                default:
                    throw new NotFoundException($"Action {action} is not known ({_identifier})");                
            }
        }

        protected override void HandleMessageInternal(BaseMessage message, string action, ListsRepository repository)
        {
            switch(message)
            {
                case ListCreatedMessage createdMessage:
                    var model = _mapper.Map<ListCreatedMessage, ListModel>(createdMessage);
                    repository.CreateListAsync(model).GetAwaiter().GetResult();
                    break;

                case ListUpdatedMessage updatedMessage:
                    var updatedModel = _mapper.Map<ListUpdatedMessage, ListModel>(updatedMessage);
                    repository.CreateOrUpdateListAsync(updatedModel).GetAwaiter().GetResult();
                    break;                
                
                case ListDeletedMessage _:
                    //nothing to do for now
                    break;

                default:
                    ThrowUnknownMessageException(message);
                    break;
            }
        }
    }
}