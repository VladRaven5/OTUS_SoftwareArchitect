using System;

namespace OTUS_SoftwareArchitect_Client.Networking.Misc
{
    internal class ThrowErrorHandler : IErrorHandler
    {
        public void HandleError(Exception ex)
        {
            throw ex;
        }
    }
}