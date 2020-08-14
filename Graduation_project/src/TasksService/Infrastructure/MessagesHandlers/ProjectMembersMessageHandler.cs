using System;
using AutoMapper;
using Shared;

namespace TasksService
{
    public class ProjectMembersMessageHandler : DomainMessagesHandlerBase<ProjectMembersRepository>
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

        protected override void HandleMessageInternal(BaseMessage message, string action, ProjectMembersRepository repository)
        {
            switch(message)
            {
                case ProjectMemberCreatedUpdatedMessage createdUpdatedMessage:
                    var model = _mapper.Map<ProjectMemberCreatedUpdatedMessage, ProjectMemberModel>(createdUpdatedMessage);
                    repository.CreateProjectMemberAsync(model.UserId, model.ProjectId).GetAwaiter().GetResult();
                    break;
                
                case ProjectMemberDeletedMessage deletedMessage:
                    var deletingModel = _mapper.Map<ProjectMemberDeletedMessage, ProjectMemberModel>(deletedMessage);
                    repository.DeleteProjectMemberAsync(deletingModel.UserId, deletingModel.ProjectId).GetAwaiter().GetResult();
                    break;

                default:
                    ThrowUnknownMessageException(message);
                    break;
            }
        }
    }
}