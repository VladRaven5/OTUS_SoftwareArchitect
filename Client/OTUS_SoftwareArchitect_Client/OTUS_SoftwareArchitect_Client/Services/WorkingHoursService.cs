using OTUS_SoftwareArchitect_Client.DTO.WorkingHours;
using OTUS_SoftwareArchitect_Client.Models;
using OTUS_SoftwareArchitect_Client.Networking;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.Services
{
    public class WorkingHoursService
    {
        private readonly WebApiClient _webApiClient;
        public WorkingHoursService()
        {
            _webApiClient = DependencyService.Resolve<WebApiClient>();
        }

        public Task<RequestResult<IEnumerable<WorkingHoursRecordModel>>> GetMyWorkingHoursAsync()
        {
            return _webApiClient.ExecuteRequestAsync(webApi => webApi.GetMyWorkingHours());
        }

        public Task<RequestResult<WorkingHoursRecordModel>> CreateWorkingHoursRecord(string requestId,
            CreateWorkingHoursRecordDto dto)
        {
            return _webApiClient.ExecuteRequestAsync(webApi => webApi.CreateWorkingHoursRecord(requestId, dto));
        }

        public Task<RequestResult<WorkingHoursRecordModel>> UpdateWorkingHoursRecord(UpdateWorkingHoursRecordDto dto)
        {
            return _webApiClient.ExecuteRequestAsync(webApi => webApi.UpdateWorkingHoursRecord(dto));
        }

        public Task<RequestResult> DeleteWorkingHoursRecord(string recordId)
        {
            return _webApiClient.ExecuteRequestAsync(webApi => webApi.DeleteWorkingHoursRecord(recordId));
        }
    }
}
