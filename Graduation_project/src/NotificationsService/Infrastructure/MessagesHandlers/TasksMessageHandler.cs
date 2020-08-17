using System;
using System.Linq;
using AutoMapper;
using Shared;

namespace NotificationsService
{
    public class TasksMessageHandler : MessagesNotificationHandlerBase<ProjectMembersRepository>
    {
        protected override string _identifier => "Tasks";

        public TasksMessageHandler(IServiceProvider serviceProvider, IMapper mapper)
            : base(serviceProvider, mapper)
        {
        }

        protected override Type GetMessageTypeForAction(string action)
        {
            switch(action)
            {
                case MessageActions.Created:
                    return typeof(TaskCreatedMessage);
                case MessageActions.Updated:
                    return typeof(TaskUpdatedMessage);                  
                case MessageActions.Deleted:
                    return typeof(TaskDeletedMessage);

                default:
                    throw new NotFoundException($"Action {action} is not known ({_identifier})");                
            }
        }

        protected override void HandleNotificationInternal(BaseMessage message, string action, ProjectMembersRepository projectMembersRepository,
            NotificationsRepository notificationsRepository)
        {
            string text = null;
            string projectId = null;

            switch(message)
            {
                case TaskCreatedMessage createdMessage:
                    projectId = createdMessage.ProjectId;

                    if(action == MessageActions.Created)
                    {
                        text = $"Created task \"{createdMessage.Title}\" in list \"{createdMessage.ListTitle}\" (project \"{createdMessage.ProjectTitle}\").";
                        
                        if(createdMessage.Members?.Any() ?? false)
                        {
                            text += $" Members: {string.Join(", ", createdMessage.Members.Select(m => m.Username))}.";
                        }

                        if(createdMessage.Labels?.Any() ?? false)
                        {
                            text += $" Labels: {string.Join(", ", createdMessage.Labels.Select(m => m.Title))}.";
                        }         
                    }                
                    break;

                case TaskUpdatedMessage updatedMessage:
                    projectId = updatedMessage.ProjectId;
                    text = $"Updated task \"{updatedMessage.Title}\" in list \"{updatedMessage.ListTitle}\" (project \"{updatedMessage.ProjectTitle}\").";  
                    
                    if(updatedMessage.AddedMembers?.Any() ?? false)
                    {
                        text += $" Added members: {string.Join(", ", updatedMessage.AddedMembers.Select(m => m.Username))}.";
                    }

                    if(updatedMessage.RemovedMembers?.Any() ?? false)
                    {
                        text += $" Removed members: {string.Join(", ", updatedMessage.RemovedMembers.Select(m => m.Username))}.";
                    }

                    if(updatedMessage.AddedLabels?.Any() ?? false)
                    {
                        text += $" Added labels: {string.Join(", ", updatedMessage.AddedLabels.Select(m => m.Title))}.";
                    } 

                    if(updatedMessage.RemovedLabels?.Any() ?? false)
                    {
                        text += $" Removed labels: {string.Join(", ", updatedMessage.RemovedLabels.Select(m => m.Title))}.";
                    }
                    break;
                
                case TaskDeletedMessage deletedMessage :
                    projectId = deletedMessage.ProjectId;
                    text = $"Removed task \"{deletedMessage.Title}\"";
                    break;

                default:
                    ThrowUnknownMessageException(message);
                    break;
            }

            if(text != null && projectId != null)
            {
                var udatedProjectMembers = projectMembersRepository
                    .GetProjectMembersIdsAsync(projectId)
                    .GetAwaiter()
                    .GetResult();

                notificationsRepository
                    .AddNotificationsToUsersAsync(text, udatedProjectMembers)
                    .GetAwaiter()
                    .GetResult();
            }
        }
    }
}