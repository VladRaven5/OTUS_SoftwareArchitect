using System;
using AutoMapper;
using Shared;

namespace TasksService
{
    public class UsersMessageHandler : DomainMessagesHandlerBase<UsersRepository>
    {
        protected override string _identifier => "Users";

        public UsersMessageHandler(IServiceProvider serviceProvider, IMapper mapper)
            : base(serviceProvider, mapper)
        {
        }

        protected override void HandleMessageInternal(BaseMessage message, string action, UsersRepository repository)
        {
            switch(message)
            {
                case UserCreatedUpdatedMessage createdUpdatedMessage:
                    var model = _mapper.Map<UserCreatedUpdatedMessage, UserModel>(createdUpdatedMessage);
                    repository.CreateOrUpdateUserAsync(model).GetAwaiter().GetResult();
                    break;
                
                case UserDeletedMessage _:
                    //nothing to do for now
                    break;

                default:
                    ThrowUnknownMessageException(message);
                    break;
            }            
        }

        protected override Type GetMessageTypeForAction(string action)
        {
            switch(action)
            {
                case MessageActions.Created:
                case MessageActions.Updated:
                    return typeof(UserCreatedUpdatedMessage);
                
                case MessageActions.Deleted:
                    return typeof(UserDeletedMessage);

                default:
                    throw new NotFoundException($"Action {action} is not known ({_identifier})");                
            }
        }
    }
}