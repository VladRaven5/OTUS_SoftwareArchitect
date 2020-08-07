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

        public ProjectsManager(RequestsRepository requestsRepository,
            ProjectsRepository projectsRepository)
        {
            _requestsRepository = requestsRepository;
            _projectsRepository = projectsRepository;
        }

        public Task<IEnumerable<ProjectModel>> GetAllProjectsAsync()
        {
            return _projectsRepository.GetProjectsAsync();
        }

        public Task<ProjectModel> GetProjectByIdAsync(string projectId)
        {
            return _projectsRepository.GetProjectByIdAsync(projectId);
        }

        public async Task<ProjectModel> CreateProjectAsync(ProjectModel newProject, string requestId)
        {
            if(!(await CheckAndSaveRequestIdAsync(requestId)))
            {
                throw new AlreadyHandledException();
            }

            try
            {
                return await _projectsRepository.CreateProjectAsync(newProject);
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

            return await _projectsRepository.UpdateProjectAsync(updatingProject);
        }
        public Task DeleteProjectAsync(string projectId)
        {
            return _projectsRepository.DeleteProjectAsync(projectId);
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