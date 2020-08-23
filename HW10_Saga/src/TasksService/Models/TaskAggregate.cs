using System.Collections.Generic;
using Shared;

namespace TasksService
{
    public class TaskAggregate : TaskModel
    {
        public string ProjectTitle { get; set; }
        public string ProjectId {get; set; }
        public string ListTitle { get; set; }
        public TransactionStates? TransactionState { get; set; }
        public string TransactionMessage { get; set; }
        public IEnumerable<UserModel> Members { get; set; }
        public IEnumerable<LabelModel> Labels { get; set; }
    }
}