using OTUS_SoftwareArchitect_Client.DTO.ListDtos;
using OTUS_SoftwareArchitect_Client.Models;
using OTUS_SoftwareArchitect_Client.Networking;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.Services
{
    public class ListsService
    {
        private readonly WebApiClient _webApiClient;

        public ListsService()
        {
            _webApiClient = DependencyService.Resolve<WebApiClient>();
        }

        public Task<RequestResult<IEnumerable<ListModel>>> GetProjectLists(string projectId)
        {
            return _webApiClient.ExecuteRequestAsync(webApi => webApi.GetProjectLists(projectId));
        }

        public Task<RequestResult<ListModel>> CreateListAsync(CreateListDto dto)
        {
            string requestId = RequestIdProvider.GetRequestId();
            return _webApiClient.ExecuteRequestAsync(webApi => webApi.CreateList(requestId, dto));
        }

        public Task<RequestResult<ListModel>> UpdateListAsync(ListModel list, string newTitle)
        {
            var dto = new UpdateListDto
            {
                Id = list.Id,
                Title = newTitle,
                Version = list.Version
            };


            return _webApiClient.ExecuteRequestAsync(webApi => webApi.UpdateList(dto));
        }

        public Task<RequestResult> DeleteListAsync(string listId)
        {
            return _webApiClient.ExecuteRequestAsync(webApi => webApi.DeleteList(listId));
        }
    }
}
