using System.Collections.Generic;

namespace Shared
{
    public class BaseModelsEqualityComparer : IEqualityComparer<BaseModel>
    {
        public bool Equals(BaseModel x, BaseModel y)
        {
            if(x == null || y == null)
                return false;

            return x.Id == y.Id;   
        }

        public int GetHashCode(BaseModel obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}