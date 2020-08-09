using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared;

namespace ProjectsService
{
    public class ProjectsManager
    {
        private int _requestIdLifetimeDays = Constants.RequestIdLifetimeDays;

        private readonly RequestsRepository _requestsRepository;
        private readonly ProjectsRepository _projectsRepository;
        private readonly RabbitMqTopicManager _rabbitMq;

        public ProjectsManager(RequestsRepository requestsRepository,
            ProjectsRepository projectsRepository, RabbitMqTopicManager rabbitMq)
        {
            _requestsRepository = requestsRepository;
            _projectsRepository = projectsRepository;
            _rabbitMq = rabbitMq;
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
                        Id = Guid.NewGuid().ToString(),
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
                    Id = Guid.NewGuid().ToString(),
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
                    Id = Guid.NewGuid().ToString(),
                    ProjectId = projectId,
                }, Topics.Projects, MessageActions.Deleted);

            return _projectsRepository.DeleteProjectAsync(projectId, outboxMessage);
        }

        private async Task<bool> CheckAndSaveRequestIdAsync(string requestId)
        {
            bool isRequestAlreadyHadled = await _requestsRepository.IsRequestIdHandledAsync(requestId);
            
            if(isRequestAlreadyHadled)
                return false;

            DateTimeOffset requestIdExpiresAt = DateTimeOffset.UtcNow.AddDays(_requestIdLifetimeDays);

            await _requestsRepository.SaveRequestIdAsync(requestId, requestIdExpiresAt); 

            return true;          
        }
    }
}