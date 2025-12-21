using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SDO.Models;

namespace SDO.Services
{
    public interface IIPCSetParamService : ISetParamService
    {
        /// <summary>
        /// 取得setItem類別的系統參數
        /// </summary>
        /// <param name="setItem"></param>
        /// <param name="delFlg"></param>
        /// <returns></returns>
        Task<IList<IPCSetParamModel>> GetSysParams(string setItem, bool? delFlg, int fromWhere = 0 );

        /// <summary>
        /// 取得多組system param
        /// </summary>
        /// <param name="setItems">key:setItem；value:是否有預設值</param>
        /// <returns></returns>
        Task<Dictionary<string, object>> GetParamByItems(Dictionary<string, bool> setItems);
        
        Task<RtnResultModel> SaveParamItem(List<IPCSetParamModel> models);
    }
}
