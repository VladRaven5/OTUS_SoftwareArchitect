using Shared;

namespace TasksService
{
    public class ListModel : BaseModel
    {
        public string Title { get; set; }
        public string ProjectId { get; set; }
    }

    public class ListModelAggregate : ListModel
    {
        public string ProjectTitle { get; set; }
    }
}