using Shared;

namespace NotificationsService
{
    public class NotificationModel : BaseDatedModel
    {
        public string Text { get; set;}
        public string UserId { get; set; }
    }
}