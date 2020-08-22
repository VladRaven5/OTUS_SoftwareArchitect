namespace Shared
{
    public class LabelDeletedMessage : BaseMessage
    {
        public string LabelId { get; set; }
        public string Title { get; set; }
    }
}