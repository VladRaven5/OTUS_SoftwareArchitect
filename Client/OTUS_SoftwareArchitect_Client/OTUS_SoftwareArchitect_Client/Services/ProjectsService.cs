using OTUS_SoftwareArchitect_Client.DTO.ProjectDtos;
using OTUS_SoftwareArchitect_Client.Models;
using OTUS_SoftwareArchitect_Client.Networking;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.Services
{
    public class ProjectsService
    {
        private readonly WebApiClient _webClient;

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
    }
}
