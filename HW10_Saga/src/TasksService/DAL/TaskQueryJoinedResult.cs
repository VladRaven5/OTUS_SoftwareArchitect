using System;
using System.Collections.Generic;
using Shared;

namespace TasksService
{
    public class TaskQueryJoinedResult
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string ProjectTitle { get; set; }
        public string ListId { get; set; }
        public string ListTitle { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskState State { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string LabelId { get; set; }
        public string LabelTitle { get; set; }
        public string TransactionId { get; set; }
        public TransactionStates TransactionState { get; set; }
        public string TransactionMessage { get; set; }
        public string LabelColor { get; set; }
        public int Version { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
    } 

    public class TaskCollections
    {
        public IEnumerable<string> Members { get; set; }
        public IEnumerable<string> Labels { get; set; }
    }
}