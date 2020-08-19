using System;

namespace Shared
{
    public abstract class BaseDatedModel : BaseModel
    {
        public DateTimeOffset? CreatedDate { get; set; }

        public override void Init()
        {
            base.Init();
            CreatedDate = DateTimeOffset.UtcNow;
        }
    }
}