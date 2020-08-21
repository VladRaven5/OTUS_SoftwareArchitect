using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace TasksService
{
    public class RedisStub : IDistributedCache
    {
        public byte[] Get(string key)
        {
            return new byte[0];
        }

        public Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            return Task.FromResult(new byte[0]);
        }

        public void Refresh(string key)
        {
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {            
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }
    }

}