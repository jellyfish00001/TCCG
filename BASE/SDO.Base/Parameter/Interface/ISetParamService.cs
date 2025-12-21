using SDO.Dac.Models;
using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface ISetParamService
    {
        /// <summary>
        /// 取得system param
        /// </summary>
        /// <returns></returns>
        Task<IList<SetParamModel>> GetSysParams();

        /// <summary>
        /// 取得system param By setItem
        /// </summary>
        /// <param name="setItem"></param>
        /// <returns></returns>
        Task<IList<SetParamModel>> GetSysParams(string setItem);

        /// <summary>
        /// 取得system param By setItem and setType
        /// </summary>
        /// <param name="setItem"></param>
        /// <param name="setType"></param>
        /// <returns></returns>
        Task<SetParamModel> GetSysParam(string setItem, string setType);

        /// <summary>
        /// 取得system paramItem
        /// </summary>
        /// <returns></returns>
        Task<IList<SetParamItemModel>> GetSysParamItems();

        Task<GridModel<SetParamItemModel>> GetSysParamItems(int skip,int take,string orderByField, string dir);

        /// <summary>
        /// 取得system paramItem By setItem
        /// </summary>
        /// <param name="setItem"></param>
        /// <returns></returns>
        Task<SetParamItemModel> GetSysParamItem(string setItem);

        /// <summary>
        /// 新增system param
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<RtnResultModel> InsertSysParam(SetParamModel model);

        /// <summary>
        /// 新增system paramItem
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<RtnResultModel> InsertSysParamItem(SetParamItemModel model);

        /// <summary>
        /// 更新system param
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<RtnResultModel> UpdateSysParam(SetParamModel model);

        /// <summary>
        /// 更新system paramItem
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<RtnResultModel> UpdateSysParamItem(SetParamItemModel model);

        /// <summary>
        /// 刪除system param
        /// </summary>
        /// <param name="setItem"></param>
        /// <param name="setType"></param>
        /// <returns></returns>
        Task<RtnResultModel> DeleteSysParam(string setItem, string setType);

        /// <summary>
        /// 刪除system paramItem
        /// </summary>
        /// <param name="setItem"></param>
        /// <returns></returns>
        Task<RtnResultModel> DeleteSysParamItem(string setItem);
    }
}