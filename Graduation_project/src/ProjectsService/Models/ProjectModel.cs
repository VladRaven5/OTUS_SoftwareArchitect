using System;
using Shared;

namespace ProjectsService
{
    public class ProjectModel : BaseVersionedModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset? BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }
}