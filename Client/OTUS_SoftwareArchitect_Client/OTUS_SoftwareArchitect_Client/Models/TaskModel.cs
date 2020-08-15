﻿using OTUS_SoftwareArchitect_Client.Models.BaseModels;
using System;
using System.Collections.Generic;

namespace OTUS_SoftwareArchitect_Client.Models
{
    public class TaskModel : BaseVersionedModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ListId { get; set; }

        public TaskState State { get; set; }
        public DateTimeOffset? DueDate { get; set; }

        public string ProjectTitle { get; set; }
        public string ProjectId { get; set; }
        public string ListTitle { get; set; }
        public ICollection<TaskUserModel> Members { get; set; }
        public IEnumerable<TaskLabelModel> Labels { get; set; }
    }
}
