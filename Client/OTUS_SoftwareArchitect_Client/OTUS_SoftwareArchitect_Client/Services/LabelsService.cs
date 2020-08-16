using OTUS_SoftwareArchitect_Client.Models;
using OTUS_SoftwareArchitect_Client.Networking;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.Services
{
    public class LabelsService
    {
        private readonly WebApiClient _webApiClient;

        public LabelsService()
        {
            _webApiClient = DependencyService.Resolve<WebApiClient>();
        }


        public Task<RequestResult<IEnumerable<LabelModel>>> GetLabels()
        {
            return _webApiClient.ExecuteRequestAsync(webApi => webApi.GetLabels());
        }
    }
}
