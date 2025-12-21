using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IProjectSituationDac : IDac
    {
        /// <summary>
        /// 取得參採情形/結案成果填報結果
        /// </summary>
        /// <param name="PLAN_NO"></param>
        /// <returns></returns>
        Task<ResSituationModel> GetRDResSituation(string PLAN_NO);

        /// <summary>
        /// 取得續列管一年內參採情形
        /// </summary>
        Task<ResSituationModel> GetRDResSituaContinue(string PLAN_NO);

        /// <summary>
        /// 儲存參採情形/結案成果填報結果
        /// </summary>
        Task UpdateRDResSituation(ResSituationModel model);

        /// <summary>
        /// 儲存續列管一年內參採情形
        /// </summary>
        Task<bool> UpdateRDResSituaContinue(ResSituationModel model);

        /// <summary>
        /// 建立參採情形/結案成果填報結果
        /// </summary>
        /// <param name="model">參採情形 Model</param>
        /// <returns></returns>
        Task InsertRDResSituation(ResSituationModel model);

        /// <summary>
        /// 建立續列管一年內參採情形
        /// </summary>
        /// <param name="model">參採情形 Model</param>
        /// <returns></returns>
        Task InsertRDResSituaContinue(ResSituationModel model);
    }
}
