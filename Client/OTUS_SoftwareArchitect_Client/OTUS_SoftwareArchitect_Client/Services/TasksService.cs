using OTUS_SoftwareArchitect_Client.Models.TaskModels;
using OTUS_SoftwareArchitect_Client.Networking;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.Services
{
    public class TasksService
    {
        private readonly WebApiClient _webClient;

        public TasksService()
        {
            _webClient = DependencyService.Resolve<WebApiClient>();
        }

        public Task<RequestResult<IEnumerable<TaskModel>>> GetMyTasksAsync()
        {
            string userId = "2                                   ";// AuthDataProvider.GetUserId();
            return _webClient.ExecuteRequestAsync(webApi => webApi.GetMyTasks());
        }
    }
}
