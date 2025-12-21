using System.Threading.Tasks;

namespace SDO.Utils
{
    public interface ICache
    {
        Task<byte[]> GetCache(string key);
        Task<string> GetStringCache(string key);
        Task SetCache(string key, byte[] value);
        Task SetStringCache(string key, string value);
        /// <summary>
        /// 刪除快取
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task DelCache(string key);

        /// <summary>
        /// 刷新快取
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task RefreshCache(string key);

    }
}