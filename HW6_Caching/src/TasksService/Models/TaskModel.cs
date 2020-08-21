using System;
using Shared;

namespace TasksService
{
    public class TaskModel : BaseVersionedModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ListId { get; set; }

        public TaskState State { get; set; }
        public DateTimeOffset? DueDate { get; set; }

        public override void Init()
        {
            base.Init();
            State = TaskState.Proposed;
        }
    }
}