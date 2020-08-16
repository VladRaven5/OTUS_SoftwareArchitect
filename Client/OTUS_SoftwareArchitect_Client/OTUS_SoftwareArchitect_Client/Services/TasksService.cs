using OTUS_SoftwareArchitect_Client.DTO.TaskDtos;
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
            return _webClient.ExecuteRequestAsync(webApi => webApi.GetMyTasks());
        }


        public Task<RequestResult<TaskModel>> CreateTaskAsync(string requestId, CreateTaskDto dto)
        {
            return _webClient.ExecuteRequestAsync(webApi => webApi.CreateTask(requestId, dto));
        }


        public Task<RequestResult<TaskModel>> UpdateTaskAsync(UpdateTaskDto dto)
        {
            return _webClient.ExecuteRequestAsync(webApi => webApi.UpdateTask(dto));
        }

        public Task<RequestResult<string>> DeleteTaskAsync(string taskId)
        {
            return _webClient.ExecuteRequestAsync(webApi => webApi.DeleteTask(taskId));
        }
    }
}
