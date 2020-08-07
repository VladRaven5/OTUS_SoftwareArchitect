using Shared;

namespace WorkingHoursService
{
    public class TaskModel : BaseModel
    {
        public string Title { get; set; }
        public string ProjectId { get; set; }
    }
}