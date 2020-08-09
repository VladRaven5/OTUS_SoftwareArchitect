using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared;

namespace ProjectMembersService
{
    public class ProjectMembersManager : DomainManagerBase
    {
        private readonly RequestsRepository _requestsRepository;
        private readonly ProjectsRepository _projectsRepository;
        private readonly UsersRepository _usersRepository;
        private readonly ProjectMembersRepository _projectMembersRepository;

        public ProjectMembersManager(RequestsRepository requestsRepository,
            ProjectsRepository projectsRepository, UsersRepository usersRepository,
            ProjectMembersRepository projectMembersRepository) : base(requestsRepository)
        {
            _requestsRepository = requestsRepository;
            _projectsRepository = projectsRepository;
            _usersRepository = usersRepository;
            _projectMembersRepository = projectMembersRepository;
        }

        ///
        /// All projects of the member or all members of the project
        ///
        public Task<IEnumerable<ProjectMemberAggregate>> GetProjectsMembersAsync(string projectId = null, string userId = null)
        {
            return _projectMembersRepository.GetProjectsMembersAsync(projectId, userId);
        }

        public Task<ProjectMemberAggregate> GetProjectMemberAsync(string projectId, string userId)
        {
            return _projectMembersRepository.GetProjectMemberAsync(projectId, userId);
        }

        public async Task AddMemberToProjectAsync(ProjectMemberModel newProjectMember, string requestId)
        {
            if(!(await CheckAndSaveRequestIdAsync(requestId)))
            {
                throw new AlreadyHandledException();
            }

            await EnsureProjectAndUserExistAsync(newProjectMember.ProjectId, newProjectMember.UserId);          

            try
            {
                var existingMember = await _projectMembersRepository.GetProjectMemberAsync(newProjectMember.ProjectId, newProjectMember.UserId);
                if(existingMember != null)
                {
                    throw new EntityExistsException("This member already in project");
                }
                await _projectMembersRepository.AddMemberToProjectAsync(newProjectMember);
            }
            catch(Exception)
            {
                //rollback request id
                await _requestsRepository.DeleteRequestIdAsync(requestId);
                throw;
            }
        }        

        public async Task UpdateProjectMebmerRoleAsync(ProjectMemberModel updatingMember)
        {
            ProjectMemberModel currentMember = await _projectMembersRepository
                .GetProjectMemberAsync(updatingMember.ProjectId, updatingMember.UserId);

            if(currentMember == null)
            {
                throw new NotFoundException($"User with id = {updatingMember.UserId} not found in project with id {updatingMember.ProjectId}");
            }

            await EnsureProjectAndUserExistAsync(updatingMember.ProjectId, updatingMember.UserId);

            await _projectMembersRepository.UpdateProjectMemberAsync(updatingMember);
        }

        public Task RemoveMemberFromProjectAsync(string projectId, string userId)
        {
            return _projectMembersRepository.DeleteMemberFromProjectAsync(projectId, userId);
        }

        private async Task EnsureProjectAndUserExistAsync(string projectId, string userId)
        {
            Task<ProjectModel> checkProjectTask = _projectsRepository.GetProjectAsync(projectId);
            Task<UserModel> checkUserTask = _usersRepository.GetUserAsync(userId);

            await Task.WhenAll(checkProjectTask, checkUserTask);

            if(checkProjectTask.Result == null)
            {
                throw new NotFoundException($"Project with id {projectId} not found");
            }

            if(checkUserTask.Result == null)
            {
                throw new NotFoundException($"User with id {userId} not found");
            }  
        }        
    }
}