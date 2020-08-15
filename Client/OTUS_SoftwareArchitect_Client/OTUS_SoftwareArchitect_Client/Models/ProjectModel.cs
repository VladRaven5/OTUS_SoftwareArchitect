using Newtonsoft.Json;
using OTUS_SoftwareArchitect_Client.Models.BaseModels;
using System;

namespace OTUS_SoftwareArchitect_Client.Models
{
    public class ProjectModel : BaseVersionedModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset? BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }

        [JsonIgnore]
        public string DateRange
        {
            get
            {
                string format = "dd.MM.yyyy";
                string from = BeginDate?.ToString(format) ?? "???";
                string to = EndDate?.ToString(format) ?? "???";
                return $"{from} - {to}";
            }
        }
    }
}
