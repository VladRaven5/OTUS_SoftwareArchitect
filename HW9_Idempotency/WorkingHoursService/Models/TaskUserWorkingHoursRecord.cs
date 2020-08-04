using System;

namespace WorkingHoursService
{
    public class TaskUserWorkingHoursRecord
    {
        public string Id { get; set; }
        public string TaskId { get; set; }
        public string UserId { get; set; }
        public double Hours { get; set; }
        public string Description { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public int Version { get; set; }
    }
}