using Shared;

namespace ListsService
{
    public class ListModel : BaseVersionedModel
    {
        public string Title { get; set; }
        public string ProjectId { get; set; }
    }
}