using OTUS_SoftwareArchitect_Client.Models.BaseModels;

namespace OTUS_SoftwareArchitect_Client.Models
{
    public class NotificationModel : BaseDatedModel
    {
        public string UserId { get; set; }
        public string Text { get; set; }
    }
}
