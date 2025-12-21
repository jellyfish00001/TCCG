using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SDO.Models;

namespace SDO.Dac
{
    public interface IIPCSetParamDac : IDac
    {
        /// <summary>
        /// 取得setItem類別的系統參數
        /// </summary>
        /// <param name="setItem"></param>
        /// <param name="delFlg"></param>
        /// <returns></returns>
        Task<IList<IPCSetParamModel>> GetSysParams(string setItem, bool? delFlg, int fromWhere = 0);
        /// <summary>
        /// 新增 SetParam資料
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        bool Insert(List<IPCSetParamModel> models);
        /// <summary>
        /// 修改 SetParam資料
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        bool Update(List<IPCSetParamModel> models);
    }
}
