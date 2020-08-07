using Shared;

namespace WorkingHoursService
{
    public class MemberWorkingHoursAggregate : TaskUserWorkingHoursRecord
    {
        public string ProjectId { get; set; }
        public string ProjectTitle { get; set; }
        public string Username { get; set; }
        public string TaskTitle { get; set; }
    }
}