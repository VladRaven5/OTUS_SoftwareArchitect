using System;

namespace Shared
{
    public abstract class BaseModel
    {
        public string Id { get; set; }        

        public virtual void Init()
        {
            Id = Guid.NewGuid().ToString();            
        }
    }
}