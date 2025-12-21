using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace SDO.Services
{
    public interface ISqlLogService
    {
        /// <summary>
        /// 取得 sql log
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<IList<SqlTraceGridModel>> ReadSqlLog(GridBasicQryModel model);
    }
}