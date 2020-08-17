namespace Shared
{
    public class LabelCreatedUpdatedMessage : BaseMessage
    {
        public string LabelId { get; set; }
        public string OldTitle { get; set; } //updated only
        public string Title { get; set; }
        public string Color { get; set; }
    }
}