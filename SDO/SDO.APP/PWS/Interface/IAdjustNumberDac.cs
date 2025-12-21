using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IAdjustNumberDac
    {

        /// <summary>
        /// 取計畫資料
        /// </summary>
        /// <returns></returns>
        Task<List<AdjustNumberModel>> GetAdjustNumber(AdjustNumberQueryModel model);

        /// <summary>
        /// 存計畫優先順序
        /// </summary>
        Task SetAdjustNumber(List<AdjustNumberModel> model);

        /// <summary>
        /// 確認是否有重複順序
        /// </summary>
        /// <param name="PLANYEAR"></param>
        /// <param name="PLANKIND"></param>
        /// <returns></returns>
        Task<bool> CheckAdjustNumber(AdjustNumberQueryModel model);

        /// <summary>
        /// 確認是否有空值
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> CheckAdjustNumberIsNull(AdjustNumberQueryModel model);

        /// <summary>
        /// 改變已審查狀態
        /// </summary>
        /// <returns></returns>
        Task PlanAdjustState(AdjustNumberQueryModel model);
    }
}
