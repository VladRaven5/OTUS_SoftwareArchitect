using System;

namespace OTUS_SoftwareArchitect_Client.Models.BaseModels
{
    public abstract class BaseDatedModel : BaseModel
    {
        public DateTimeOffset? CreatedDate { get; set; }
    }
}
