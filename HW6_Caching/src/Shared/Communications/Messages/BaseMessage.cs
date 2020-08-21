using System;

namespace Shared
{
    public abstract class BaseMessage
    {
        public string Id { get; set; }

        public virtual void Init()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}