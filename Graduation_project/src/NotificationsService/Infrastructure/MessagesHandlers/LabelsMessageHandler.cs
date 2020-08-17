using System;
using AutoMapper;
using Shared;

namespace NotificationsService
{
    public class LabelsMessageHandler : DomainMessagesHandlerBase<NotificationsRepository>
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

        protected override void HandleMessageInternal(BaseMessage message, string action, NotificationsRepository repository)
        {
            string text = null;

            switch(message)
            {
                case LabelCreatedUpdatedMessage createdUpdatedMessage:

                    
                    if(action == MessageActions.Created)
                    {
                        text = $"Created label \"{createdUpdatedMessage.Title}\" (#{createdUpdatedMessage.Color})";
                    }

                    if(action == MessageActions.Updated)
                    {
                        string renameText = string.Empty;
                        if(createdUpdatedMessage.OldTitle != createdUpdatedMessage.Title)
                        {
                            renameText = $"\"{createdUpdatedMessage.OldTitle}\" to ";
                        }

                        text = $"Updated label {renameText}\"{createdUpdatedMessage.Title}\"";
                    }                  
                    break;
                
                case LabelDeletedMessage deletedMessage :
                    text = $"Label \"{deletedMessage.Title}\" removed";
                    break;

                default:
                    ThrowUnknownMessageException(message);
                    break;
            }

            if(text != null)
            {
                repository
                    .AddNotificationsToAllUsersAsync(text)
                    .GetAwaiter()
                    .GetResult();
            }
        }
    }
}