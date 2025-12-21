using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface ISCApplicationDac
    {
        /// <summary>
        /// 取得系統路徑
        /// </summary>
        /// <param name="apId">系統代碼</param>
        /// <returns></returns>
        Task<string> GetPrgPath(string apId);

        /// <summary>
        /// 新增 Session資訊
        /// </summary>
        /// <param name="model"></param>
        void InsertScSession(SCSessionModel model);

        /// <summary>
        /// 取得先期計畫數量
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        Task<int> GetPWSSDCnt(string orgId);

        /// <summary>
        /// 取得研究發展最大年度
        /// </summary>
        /// <returns></returns>
        Task<string> GetRdMaxYear();

        /// <summary>
        /// 取得委託研究數量
        /// </summary>
        /// <param name="maxYear"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        Task<int> GetRDRPMCnt(string maxYear, string orgId);

        /// <summary>
        /// 取得創新提案數量
        /// </summary>
        /// <param name="rdInnYear"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        Task<int> GetRDWIPCnt(string rdInnYear, string orgId);

    }
}
