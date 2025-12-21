using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IProjectSituationService
    {
        /// <summary>
        /// 取得參採情形
        /// </summary>
        Task<ResSituationModel> GetRDResSituation(string planNo);

        /// <summary>
        /// 取得續列管一年內參採情形
        /// </summary>
        Task<ResSituationModel> GetRDResSituaContinue(string planNo);

        /// <summary>
        /// 儲存參採情形
        /// </summary>
        Task SaveRDResSituation(ResSituationModel model);

        /// <summary>
        /// 儲存續列管一年內參採情形
        /// </summary>
        Task SaveRDResSituaContinue(ResSituationModel model);
    }
}
