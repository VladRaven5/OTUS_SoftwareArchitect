using System.Collections.Generic;

namespace Shared
{
    public class TaskCreatedMessage : BaseMessage
    {
        public string TaskId { get; set; }
        public string Title { get; set; }
        public string ProjectId { get; set; }
        public string ProjectTitle { get; set; }
        public string ListId { get; set; }
        public string ListTitle { get; set; }
        public IEnumerable<TaskUserRecord> Members { get; set; }
        public IEnumerable<TaskLabelRecord> Labels { get; set; }
    }

    public class TaskUserRecord
    {
        public string UserId { get; set; }
        public string Username { get; set; }        
    }

    public class TaskLabelRecord
    {
        public string LabelId { get; set; }
        public string Title { get; set; }
    }
}