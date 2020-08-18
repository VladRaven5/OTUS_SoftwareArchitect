using OTUS_SoftwareArchitect_Client.Models.BaseModels;

namespace OTUS_SoftwareArchitect_Client.Models
{
    public class WorkingHoursRecordModel : BaseVersionedModel
    {
        public string TaskId { get; set; }
        public string UserId { get; set; }
        public double Hours { get; set; }
        public string Description { get; set; }
        public string ProjectId { get; set; }
        public string ProjectTitle { get; set; }
        public string Username { get; set; }
        public string TaskTitle { get; set; }
    }
}
