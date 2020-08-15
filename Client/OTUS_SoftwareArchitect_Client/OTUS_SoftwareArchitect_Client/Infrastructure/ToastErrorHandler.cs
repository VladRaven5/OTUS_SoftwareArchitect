using Acr.UserDialogs;
using OTUS_SoftwareArchitect_Client.Networking.Misc;
using System;

namespace OTUS_SoftwareArchitect_Client.Infrastructure
{
    class ToastErrorHandler : IErrorHandler
    {
        public void HandleError(Exception ex)
        {
            UserDialogs.Instance.Toast(new ToastConfig(ex.Message));
        }
    }
}
