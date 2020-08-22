namespace Shared
{
    public class ListUpdatedMessage : BaseMessage
    {
        public string ListId { get; set; }
        public string Title { get; set; }
    }
}