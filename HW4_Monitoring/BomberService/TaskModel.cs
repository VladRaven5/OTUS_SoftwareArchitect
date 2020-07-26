using System;

namespace BomberService
{
    public class TaskModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string AssignedTo { get; set; }
        public TaskState State { get; set; }
    }

    public enum TaskState
    {
        Proposed = 0,
        Active = 1,
        Resolved = 2,
        Closed = 3,
    }
}