using System;
using AutoMapper;
using Shared;

namespace NotificationsService
{
    public class ProjectMembersMessageHandler : MessagesNotificationHandlerBase<ProjectMembersRepository>
    {
        protected override string _identifier => "ProjectsMembers";

        public ProjectMembersMessageHandler(IServiceProvider serviceProvider, IMapper mapper)
            : base(serviceProvider, mapper)
        {
        }

        protected override Type GetMessageTypeForAction(string action)
        {
            switch(action)
            {
                case MessageActions.Created:
                case MessageActions.Updated:
                    return typeof(ProjectMemberCreatedUpdatedMessage);
                
                case MessageActions.Deleted:
                    return typeof(ProjectMemberDeletedMessage);

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
                case ProjectMemberCreatedUpdatedMessage createdUpdatedMessage:
                    projectId = createdUpdatedMessage.ProjectId;
                    var model = _mapper.Map<ProjectMemberCreatedUpdatedMessage, ProjectMemberModel>(createdUpdatedMessage);
                    projectMembersRepository.CreateProjectMemberAsync(model.UserId, model.ProjectId).GetAwaiter().GetResult();                    

                    if(action == MessageActions.Created)
                    {
                        text = $"Member {createdUpdatedMessage.Username} added to project \"{createdUpdatedMessage.ProjectTitle}\"";                        
                    }
                    break;
                
                case ProjectMemberDeletedMessage deletedMessage:
                    projectId = deletedMessage.ProjectId;
                    var deletingModel = _mapper.Map<ProjectMemberDeletedMessage, ProjectMemberModel>(deletedMessage);
                    projectMembersRepository.DeleteProjectMemberAsync(deletingModel.UserId, deletingModel.ProjectId).GetAwaiter().GetResult();
                    text = $"Member {deletedMessage.Username} removed from project \"{deletedMessage.ProjectTitle}\""; 
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