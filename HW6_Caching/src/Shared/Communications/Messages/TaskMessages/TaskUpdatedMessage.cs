using System.Collections.Generic;

namespace Shared
{
    public class TaskUpdatedMessage : BaseMessage
    {
        public string TaskId { get; set; }
        public string Title { get; set; }
        public string ProjectId { get; set; }
        public string ProjectTitle { get; set; }
        public string ListId { get; set; }
        public string ListTitle { get; set; }
        public IEnumerable<TaskUserRecord> RemovedMembers { get; set; }
        public IEnumerable<TaskLabelRecord> RemovedLabels { get; set; }
        public IEnumerable<TaskUserRecord> AddedMembers { get; set; }
        public IEnumerable<TaskLabelRecord> AddedLabels { get; set; }
    }
}