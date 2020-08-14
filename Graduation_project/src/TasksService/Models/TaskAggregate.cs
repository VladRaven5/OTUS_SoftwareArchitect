using System.Collections.Generic;

namespace TasksService
{
    public class TaskAggregate : TaskModel
    {
        public string ProjectTitle { get; set; }
        public string ProjectId {get; set; }
        public string ListTitle { get; set; }
        public IEnumerable<UserModel> Members { get; set; }
        public IEnumerable<LabelModel> Labels { get; set; }
    }
}