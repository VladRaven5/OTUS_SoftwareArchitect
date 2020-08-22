using System.Collections.Generic;
using System.Linq;

namespace Shared
{
    public static class CollectionsHelper
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || collection.Count() == 0;
        }        
    }
}