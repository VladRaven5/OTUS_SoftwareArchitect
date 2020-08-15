namespace OTUS_SoftwareArchitect_Client.Models.BaseModels
{
    public abstract class BaseVersionedModel : BaseDatedModel
    {
        public int Version { get; set; }
    }
}
