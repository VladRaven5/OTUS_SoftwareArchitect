namespace Shared
{
    public abstract class BaseVersionedModel : BaseDatedModel
    {
        public int Version { get; set; }

        public override void Init()
        {
            base.Init();
            Version = 1;
        }
    }
}