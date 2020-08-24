using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace TasksService
{
    public static class CacheHelpers
    {
        public static async Task<TObject> TryGetCachedValueAsync<TObject>(this IDistributedCache cache, string key)
            where TObject : class
        {
            string cachedString = await cache.GetStringAsync(key);
            if(string.IsNullOrWhiteSpace(cachedString))
                return null;

            Console.WriteLine($"Cache: Value for key {key} found");

            return JsonConvert.DeserializeObject<TObject>(cachedString);            
        }

        public static Task SetCacheValueAsync<TObject>(this IDistributedCache cache, string key, TObject value,
            TimeSpan lifetime) where TObject : class
        {
            string cachingString = JsonConvert.SerializeObject(value);
            var opt = new DistributedCacheEntryOptions().SetAbsoluteExpiration(lifetime);
            return cache.SetStringAsync(key, cachingString, opt);
        }
    }
}