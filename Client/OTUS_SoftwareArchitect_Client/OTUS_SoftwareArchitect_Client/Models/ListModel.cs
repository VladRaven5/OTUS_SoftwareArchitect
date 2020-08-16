using OTUS_SoftwareArchitect_Client.Models.BaseModels;

namespace OTUS_SoftwareArchitect_Client.Models
{
    public class ListModel : BaseVersionedModel
    {
        public string Title { get; set; }
        public string ProjectId { get; set; }
        public string ProjectTitle { get; set; }
    }
}
