using SDO.Dac;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SDO.Services
{
    public class AdjustNumberService : Service, IAdjustNumberService
    {
        private readonly IAdjustNumberDac dac;

        public AdjustNumberService(IAdjustNumberDac dac)
        {
            this.dac = dac;
        }

        /// <summary>
        /// 取計畫資料
        /// </summary>
        /// <returns></returns>
        public async Task<List<AdjustNumberModel>> GetAdjustNumber(AdjustNumberQueryModel model)
        {
            return await dac.GetAdjustNumber(model);
        }

        /// <summary>
        /// 計畫資料送審
        /// </summary>
        /// <returns></returns>
        public async Task<string> SetAdjustNumber(AdjustNumberQueryModel model)
        {
            string Message = "送審失敗，請檢察優先順序編號重複或未填寫";
            using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                // 設定計畫資料
                await dac.SetAdjustNumber(model.AdjustNumberModels);
                // 如果是送審，則檢查是否有資料異動
                if (model.SaveType == "sendReview")
                {
                    // 確認是否有重複順序
                    bool isSort = await dac.CheckAdjustNumber(model);
                    bool isNull = await dac.CheckAdjustNumberIsNull(model);
                    if (isSort && isNull)
                    {
                        // (要改為計畫送審狀態)
                        await dac.PlanAdjustState(model);
                        Message = "送審成功";
                    }
                }
                else if (model.SaveType == "save")
                {
                    Message = "存檔成功";
                }
                scope.Complete();
            }
            return Message;
        }
    }
}
