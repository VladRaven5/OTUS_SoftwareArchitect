using System;
using AutoMapper;
using Shared;

namespace TasksService
{
    public class ProjectsMessageHandler : DomainMessagesHandlerBase<ProjectsRepository>
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

        protected override void HandleMessageInternal(BaseMessage message, string action, ProjectsRepository repository)
        {
            switch(message)
            {
                case ProjectCreatedUpdatedMessage createdUpdatedMessage:
                    var model = _mapper.Map<ProjectCreatedUpdatedMessage, ProjectModel>(createdUpdatedMessage);
                    repository.CreateOrUpdateProjectAsync(model).GetAwaiter().GetResult();
                    break;
                
                case ProjectDeletedMessage _:
                    //nothing to do for now
                    break;

                default:
                    ThrowUnknownMessageException(message);
                    break;
            }
        }
    }
}