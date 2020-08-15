using System;
using System.Collections.Generic;
using System.Text;

namespace OTUS_SoftwareArchitect_Client.Networking.Misc
{
    public interface IErrorHandler
    {
        void HandleError(Exception ex);
    }
}
