using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IApiLogService
    {
        /// <summary>
        /// GetApiLog From DB
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<IList<ApiTraceModel>> GetApiLog(GridBasicQryModel model);
    }
}