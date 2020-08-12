using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared;

namespace ProjectsService
{
    public class ProjectsManager : DomainManagerBase
    {
        private readonly RequestsRepository _requestsRepository;
        private readonly ProjectsRepository _projectsRepository;

        public ProjectsManager(RequestsRepository requestsRepository,
            ProjectsRepository projectsRepository, RabbitMqTopicManager rabbitMq) : base(requestsRepository)
        {
            _requestsRepository = requestsRepository;
            _projectsRepository = projectsRepository;
        }

        public Task<IEnumerable<ProjectModel>> GetAllProjectsAsync()
        {
            return _projectsRepository.GetProjectsAsync();
        }

        public async Task<ProjectModel> GetProjectByIdAsync(string projectId)
        {
             var project = await _projectsRepository.GetProjectByIdAsync(projectId);
             if(project == null)
             {
                 throw new NotFoundException($"Project with id {projectId} not found");
             }
             return project;
        }

        public async Task<ProjectModel> CreateProjectAsync(ProjectModel newProject, string requestId)
        {
            if(!(await CheckAndSaveRequestIdAsync(requestId)))
            {
                throw new AlreadyHandledException();
            }

            try
            {
                newProject.Init();

                var outboxMessage = OutboxMessageModel.Create(
                    new ProjectCreatedUpdatedMessage
                    {
                        ProjectId = newProject.Id,
                        Title = newProject.Title
                    }, Topics.Projects, MessageActions.Created);

                return await _projectsRepository.CreateProjectAsync(newProject, outboxMessage);
            }
            catch(Exception)
            {
                //rollback request id
                await _requestsRepository.DeleteRequestIdAsync(requestId);
                throw;
            }
        }

        public async Task<ProjectModel> UpdateProjectAsync(ProjectModel updatingProject)
        {
            ProjectModel currentProject = await _projectsRepository.GetProjectByIdAsync(updatingProject.Id);
            if(currentProject == null)
            {
                throw new NotFoundException($"Project with id = {updatingProject.Id} not found");
            }

            if(currentProject.Version != updatingProject.Version)
            {
                throw new VersionsNotMatchException();
            }

            var outboxMessage = OutboxMessageModel.Create(
                new ProjectCreatedUpdatedMessage
                {
                    ProjectId = updatingProject.Id,
                    Title = updatingProject.Title
                }, Topics.Projects, MessageActions.Updated);

            return await _projectsRepository.UpdateProjectAsync(updatingProject, outboxMessage);
        }
        public Task DeleteProjectAsync(string projectId)
        {
            var outboxMessage = OutboxMessageModel.Create(
                new ProjectDeletedMessage
                {
                    ProjectId = projectId,
                }, Topics.Projects, MessageActions.Deleted);

            return _projectsRepository.DeleteProjectAsync(projectId, outboxMessage);
        }        
    }
}