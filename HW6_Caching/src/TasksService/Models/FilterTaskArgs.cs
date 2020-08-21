using System.Collections.Generic;

namespace TasksService
{
    public class FilterTaskArgs
    {
        public string Title { get; set; }
        public IEnumerable<string> UsersIds { get; set; }
        public IEnumerable<string> LabelsIds { get; set; }
        public IEnumerable<string> ListsIds { get; set; }
        public IEnumerable<TaskState> States { get; set; }
    }
}