using System.Threading.Tasks;

namespace SDO.Services
{
    public interface ISystemInfoService
    {
        string GetCacheExpireTime();
        Task<string> GetVersionInfo();
    }
}
