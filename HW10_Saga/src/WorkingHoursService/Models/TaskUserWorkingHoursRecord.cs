using Shared;

namespace WorkingHoursService
{
    public class TaskUserWorkingHoursRecord : BaseVersionedModel
    {
        public string TaskId { get; set; }
        public string UserId { get; set; }
        public double Hours { get; set; }
        public string Description { get; set; }
    }
}