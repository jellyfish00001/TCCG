using SDO.Models;
using System.Threading.Tasks;

namespace SDO.Utils
{
    public interface IApiTrace
    {
        Task AddTrace(ApiTraceModel apiTrace);
        Task<string> MD5Encode(string userPd);
    }
}