﻿using OTUS_SoftwareArchitect_Client.Models;
using OTUS_SoftwareArchitect_Client.Networking;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.Services
{
    public class NotificationsService
    {
        private readonly WebApiClient _webApiClient;

        public NotificationsService()
        {
            _webApiClient = DependencyService.Resolve<WebApiClient>();
        }

        public Task<RequestResult<IEnumerable<NotificationModel>>> GetMyNotificationsAsync()
        {
            return _webApiClient.ExecuteRequestAsync(webApi => webApi.GetMyNotifications());
        }
    }
}
