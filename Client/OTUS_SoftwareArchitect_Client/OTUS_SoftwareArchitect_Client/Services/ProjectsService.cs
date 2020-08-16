using OTUS_SoftwareArchitect_Client.DTO.ProjectDtos;
using OTUS_SoftwareArchitect_Client.Helpers;
using OTUS_SoftwareArchitect_Client.Models;
using OTUS_SoftwareArchitect_Client.Models.BaseModels;
using OTUS_SoftwareArchitect_Client.Models.ProjectModels;
using OTUS_SoftwareArchitect_Client.Networking;
using OTUS_SoftwareArchitect_Client.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.Services
{
    public class ProjectsService
    {
        private readonly WebApiClient _webClient;
        private readonly IEqualityComparer<BaseModel> _modelsByIdComparer = new ModelsByIdComparer();

        public ProjectsService()
        {
            _webClient = DependencyService.Resolve<WebApiClient>();
        }

        public Task<RequestResult<IEnumerable<ProjectModel>>> GetProjectsAsync()
        {
            return _webClient.ExecuteRequestAsync(webApi => webApi.GetProjects());
        }

        public Task<RequestResult<ProjectModel>> CreateProjectAsync(string requestId, CreateProjectDto dto)
        {
            return _webClient.ExecuteRequestAsync(webApi => webApi.CreateProject(requestId, dto));
        }

        public Task<RequestResult<ProjectModel>> GetProjectAsync(string projectId)
        {
            return _webClient.ExecuteRequestAsync(webApi => webApi.GetProject(projectId));
        }

        public async Task<RequestResult<ProjectModel>> UdpateProjectAsync(UpdateProjectDto projectDto, List<SimpleUserModel> oldMembersCollection,
            List<SimpleUserModel> newMembersCollection)
        {
            var projectUpdateTask = _webClient.ExecuteRequestAsync(webApi => webApi.UpdateProject(projectDto));

            var addedMembersIds = newMembersCollection.Except(oldMembersCollection, _modelsByIdComparer).Select(u => u.Id).ToList();
            var removedMembersIds = oldMembersCollection.Except(newMembersCollection, _modelsByIdComparer).Select(u => u.Id).ToList();

            var tasks = new List<Task>();
            tasks.Add(projectUpdateTask);

            foreach(var addedMemberId in addedMembersIds)
            {
                var addDto = new CreateUpdateProjectMemberDto
                {
                    UserId = addedMemberId,
                    ProjectId = projectDto.Id,
                    Role = (int)ProjectMemberRole.Implementer
                };

                string requestId = RequestIdProvider.GetRequestId();

                var addMemberTask = _webClient.ExecuteRequestAsync(webApi => webApi.AddMemberToProject(requestId, addDto));

                tasks.Add(addMemberTask);
            }

            foreach(var removingMemberId in removedMembersIds)
            {
                var removeDto = new DeleteProjectMemberDto
                {
                    UserId = removingMemberId,
                    ProjectId = projectDto.Id
                };

                string requestId = RequestIdProvider.GetRequestId();

                var removeMemberTask = _webClient.ExecuteRequestAsync(webApi => webApi.RemoveMemberFromProject(removeDto));

                tasks.Add(removeMemberTask);
            }

            await Task.WhenAll(tasks);

            foreach(var task in tasks)
            {
                task.ThrowIfFaulted();
            }

            return projectUpdateTask.Result;            
        }
        
        public Task<RequestResult> DeleteProjectAsync(string projectId)
        {
            return _webClient.ExecuteRequestAsync(webApi => webApi.DeleteProject(projectId));
        }

        public Task<RequestResult<IEnumerable<ProjectMemberModel>>> GetProjectMembers(string projectId)
        {
            return _webClient.ExecuteRequestAsync(webApi => webApi.GetProjectMembers(projectId));
        }


        private class ModelsByIdComparer : IEqualityComparer<BaseModel>
        {
            public bool Equals(BaseModel x, BaseModel y)
            {
                return x?.Id == y?.Id;
            }

            public int GetHashCode(BaseModel obj)
            {
                return obj.Id.GetHashCode();
            }
        }
    }
}
