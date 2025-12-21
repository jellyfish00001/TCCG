using SDO.Dac;
using SDO.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using SDO.Utils;
using SDO.Dac.Models;

namespace SDO.Services
{
    public class SetParamService : Service, ISetParamService
    {
        private readonly ISetParamDac dac;
        private readonly ISysParam sysParam;
        public SetParamService(ISetParamDac dac, ISysParam sysParam)
        {
            this.dac = dac;
            this.sysParam = sysParam;
        }

        /// <summary>
        /// 以setItem setType查詢系統參數
        /// </summary>
        /// <param name="setItem"></param>
        /// <param name="setType"></param>
        /// <returns></returns>
        public async Task<SetParamModel> GetSysParam(string setItem, string setType)
        {
            return await sysParam.GetSysParam(setItem, setType);
        }

        /// <summary>
        /// 取得setItem類別的系統參數
        /// </summary>
        /// <param name="setItem"></param>
        /// <returns></returns>
        public virtual async Task<IList<SetParamModel>> GetSysParams(string setItem)
        {
            return await sysParam.GetSysParams(setItem);
        }

        /// <summary>
        /// 取得系統參數
        /// </summary>
        /// <returns></returns>
        public async Task<IList<SetParamModel>> GetSysParams()
        {
            return await sysParam.GetSysParams();
        }

        /// <summary>
        /// 取得系統參數類別
        /// </summary>
        /// <returns></returns>
        public async Task<IList<SetParamItemModel>> GetSysParamItems()
        {
            return await sysParam.GetSysParamItems();
        }

        /// <summary>
        /// 取得系統參數類別
        /// </summary>
        /// <returns></returns>
        public async Task<GridModel<SetParamItemModel>> GetSysParamItems(int skip,int take, string orderByField, string dir)
        {
            return await sysParam.GetSysParamItems(skip,take, orderByField,  dir);
        }

        /// <summary>
        /// 以setItem查詢系統參數類別
        /// </summary>
        /// <param name="setItem"></param>
        /// <returns></returns>
        public async Task<SetParamItemModel> GetSysParamItem(string setItem)
        {
            return await sysParam.GetSysParamItem(setItem);
        }

        /// <summary>
        /// 新增系統參數
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> InsertSysParam(SetParamModel model)
        {
            #region check exist data
            SetParamModel check = await dac.CheckExist(model.SET_ITEM, model.SET_TYPE);
            if (check != default(SetParamModel) && !check.DEL_FLG)
            {
                return ChangeResult(ResultType.Fail | ResultType.Insert,
                    string.Format("{0}-{1}", HtmlEncode(model.SET_ITEM),
                    HtmlEncode(model.SET_TYPE)));
            }
            #endregion

            if (check == default(SetParamModel))
                await dac.Insert(model);
            else
                await dac.Update(model);

            return ChangeResult(ResultType.Success | ResultType.Insert);
        }

        /// <summary>
        /// 新增系統參數類別
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> InsertSysParamItem(SetParamItemModel model)
        {
            #region check exist data
            SetParamItemModel check = await dac.CheckItemExist(model.SET_ITEM);
            if (check != default(SetParamItemModel) && (!check.DEL_FLG || !check.EDITABLE))
            {
                return ChangeResult(ResultType.Fail | ResultType.Insert,
                    HtmlEncode(model.SET_ITEM));
            }
            #endregion

            await (check == default(SetParamItemModel))
                .IsTrue(async () => await dac.InsertItem(model))
                .IsFalse(async () => await dac.UpdateItem(model));

            return ChangeResult(ResultType.Success | ResultType.Insert);
        }

        /// <summary>
        /// 更新系統參數
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> UpdateSysParam(SetParamModel model)
        {
            await dac.Update(model);
            return ChangeResult(ResultType.Success | ResultType.Update);
        }

        /// <summary>
        /// 更新系統參數類別
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> UpdateSysParamItem(SetParamItemModel model)
        {
            await dac.UpdateItem(model);
            return ChangeResult(ResultType.Success | ResultType.Update);
        }

        /// <summary>
        /// 刪除系統參數
        /// </summary>
        /// <param name="setItem"></param>
        /// <param name="setType"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> DeleteSysParam(string setItem, string setType)
        {
            await dac.Delete(setItem, setType);
            return ChangeResult(ResultType.Success | ResultType.Delete);
        }

        /// <summary>
        /// 刪除系統參數類別
        /// </summary>
        /// <param name="setItem"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> DeleteSysParamItem(string setItem)
        {
            await dac.DeleteItem(setItem);
            return ChangeResult(ResultType.Success | ResultType.Delete);
        }
    }
}