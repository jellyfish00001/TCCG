using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Utils
{
    public class Cache : ICache
    {
        //本機快取
        //private readonly IMemoryCache memoryCache;

        //分散式快取
        private readonly IDistributedCache distributedCache;
        public Cache(IDistributedCache distributedCache)
        {
            this.distributedCache = distributedCache;
        }

        public async Task SetCache(string key, byte[] value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(key);

            //分散式快取
            await distributedCache.SetAsync(key, value);
        
            //本機快取
            //memoryCache.Set(key, value, new MemoryCacheEntryOptions() { 
            //    Size = 1024, //容量大小
            //    SlidingExpiration = TimeSpan.FromSeconds(30*60) //有效期限
            //});
        }
        /// <summary>
        /// 刪除cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task DelCache(string key)
        {
           await distributedCache.RemoveAsync(key);
        }

        public async Task<byte[]> GetCache(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(key);

            //分散式快取
            //await distributedCache.RefreshAsync(key); //刷新有效期限
            return await distributedCache.GetAsync(key);
            //本機快取
            //return memoryCache.Get<byte[]>(key)
        }

        public async Task RefreshCache(string key)
        {
            await distributedCache.RefreshAsync(key);
        }


        public async Task SetStringCache(string key, string value)
        {
            await SetCache(key, Encoding.UTF8.GetBytes(value));
        }

        public async Task<string> GetStringCache(string key)
        {
            byte[] bytes = await GetCache(key);
            return bytes == null ? string.Empty : Encoding.UTF8.GetString(bytes);
        }
    }
}
