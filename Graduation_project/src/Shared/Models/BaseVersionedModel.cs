namespace Shared
{
    public abstract class BaseVersionedModel : BaseModel
    {
        public int Version { get; set; }

        public override void Init()
        {
            base.Init();
            Version = 1;
        }
    }
}