using SDO.Dac;
using SDO.Dac.Models;
using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Utils
{
    public class SysParam : ISysParam
    {
        private readonly ISysParamDac dac;

        public SysParam(ISysParamDac dac)
        {
            this.dac = dac;
        }

        /// <summary>
        /// 以setItem setType查詢系統參數
        /// </summary>
        /// <param name="setItem"></param>
        /// <param name="setType"></param>
        /// <returns></returns>
        public async Task<SetParamModel> GetSysParam(string setItem, string setType)
        {
            return await dac.GetSysParam(setItem, setType);
        }

        /// <summary>
        /// 取得setItem類別的系統參數
        /// </summary>
        /// <param name="setItem"></param>
        /// <returns></returns>
        public async Task<IList<SetParamModel>> GetSysParams(string setItem)
        {
            return await dac.GetSysParams(setItem);
        }

        /// <summary>
        /// 取得系統參數
        /// </summary>
        /// <returns></returns>
        public async Task<IList<SetParamModel>> GetSysParams()
        {
            return await dac.GetSysParams();
        }

        /// <summary>
        /// 取得系統參數
        /// </summary>
        /// <returns></returns>
        public async Task<GridModel<SetParamItemModel>> GetSysParamItems(int skip,int take, string orderByField, string dir)
        {
            return await dac.GetSysParamItems(skip,take,  orderByField,  dir);
        }

        /// <summary>
        /// 取得系統參數類別
        /// </summary>
        /// <returns></returns>
        public async Task<IList<SetParamItemModel>> GetSysParamItems()
        {
            return await dac.GetSysParamItems();
        }

        /// <summary>
        /// 以setItem查詢系統參數類別
        /// </summary>
        /// <param name="setItem"></param>
        /// <returns></returns>
        public async Task<SetParamItemModel> GetSysParamItem(string setItem)
        {
            return await dac.GetSysParamItem(setItem);
        }
    }
}
