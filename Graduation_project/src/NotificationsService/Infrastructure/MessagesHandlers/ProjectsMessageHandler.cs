using System;
using System.Linq;
using AutoMapper;
using Shared;

namespace NotificationsService
{
    public class ProjectsMessageHandler : MessagesNotificationHandlerBase<ProjectMembersRepository>
    {
        protected override string _identifier => "Projects";

        public ProjectsMessageHandler(IServiceProvider serviceProvider, IMapper mapper)
            : base(serviceProvider, mapper)
        {
        }

        protected override Type GetMessageTypeForAction(string action)
        {
            switch(action)
            {
                case MessageActions.Created:
                case MessageActions.Updated:
                    return typeof(ProjectCreatedUpdatedMessage);
                
                case MessageActions.Deleted:
                    return typeof(ProjectDeletedMessage);

                default:
                    throw new NotFoundException($"Action {action} is not known ({_identifier})");                
            }
        }

        protected override void HandleNotificationInternal(BaseMessage message, string action, ProjectMembersRepository projectMembersRepository,
            NotificationsRepository notificationsRepository)
        {
            string text = null;

            switch(message)
            {
                case ProjectCreatedUpdatedMessage createdUpdatedMessage:

                    if(action == MessageActions.Created)
                    {
                        text = $"Created project \"{createdUpdatedMessage.Title}\"";
                        
                        notificationsRepository
                            .AddNotificationsToAllUsersAsync(text)
                            .GetAwaiter()
                            .GetResult();
                    }

                    if(action == MessageActions.Updated)
                    {
                        string renameText = string.Empty;
                        if(createdUpdatedMessage.OldTitle != createdUpdatedMessage.Title)
                        {
                            renameText = $"\"{createdUpdatedMessage.OldTitle}\" to ";
                        }

                        text = $"Updated project {renameText}\"{createdUpdatedMessage.Title}\"";

                        var udatedProjectMembers = projectMembersRepository
                            .GetProjectMembersIdsAsync(createdUpdatedMessage.ProjectId)
                            .GetAwaiter()
                            .GetResult();

                        notificationsRepository
                            .AddNotificationsToUsersAsync(text, udatedProjectMembers)
                            .GetAwaiter()
                            .GetResult();
                    }                  
                    break;
                
                case ProjectDeletedMessage deletedMessage :
                        text = $"Deleted project \"{deletedMessage.Title}\"";

                        var deletedProjectMembers = projectMembersRepository
                            .GetProjectMembersIdsAsync(deletedMessage.ProjectId)
                            .GetAwaiter()
                            .GetResult();

                        notificationsRepository
                            .AddNotificationsToUsersAsync(text, deletedProjectMembers)
                            .GetAwaiter()
                            .GetResult();
                    break;

                default:
                    ThrowUnknownMessageException(message);
                    break;
            }
        }
    }
}