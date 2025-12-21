using SDO.Dac;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SDO.Services
{
    public class SCApplicationService : Service, ISCApplicationService
    {
        private ISCApplicationDac scApplicationDac;
        private readonly ISysParam sysParam;
        private readonly IUserProfile userProfile;

        public SCApplicationService(ISCApplicationDac scApplicationDac, ISysParam sysParam, IUserProfile userProfile)
        {
            this.scApplicationDac = scApplicationDac;
            this.sysParam = sysParam;
            this.userProfile = userProfile;
        }

        /// <summary>
        /// 取得Sc連結
        /// </summary>
        /// <param name="domainName">網域名稱</param>
        /// <param name="apId">系統代號</param>
        /// <returns></returns>
        public async Task<RtnResultModel> GetScLink(string domainName, string apId)
        {
            string prgPath = (await sysParam.GetSysParam("SC", apId)).SET_VALUE;

            if (string.IsNullOrEmpty(prgPath))
            {
                return ChangeResult(false);
            }

            string risUrl = (await sysParam.GetSysParam("DOMAIN_NAME", domainName)).SET_VALUE;
            string link = Path.Combine(risUrl, prgPath);

            string userId = userProfile.GetLoginUser().USER_ID;
            if (!string.IsNullOrEmpty(userId))
            {
                SCSessionModel model = new SCSessionModel
                {
                    AP_ID = apId,
                    USR_COMP_ID = "GSS",
                    USR_ID = userId,
                    SESSION_ID = Guid.NewGuid().ToString()
                };
                
                // 新增 Session 進 SC資料庫 (讓SC網頁檢查)
                scApplicationDac.InsertScSession(model);

                link = $"{link}?strUsrID={model.USR_ID}&strCompID={model.USR_COMP_ID}&strAPID={model.AP_ID}&strSID={model.SESSION_ID}";
            }

            return ChangeResult(true, link);
        }

        /// <summary>
        /// 取得 SC 資訊
        /// </summary>
        /// <param name="orgId">組織ID</param>
        /// <returns></returns>
        public async Task<SCApplicationModel> GetScData(string orgId)
        {
            SCApplicationModel result = new SCApplicationModel();
            // 取得先期計畫數量
            result.PwssdCnt = await scApplicationDac.GetPWSSDCnt(orgId);
            // 取得研究發展最大年度
            result.RdMaxYear = await scApplicationDac.GetRdMaxYear();
            // 取得委託研究數量
            result.RdRpmCnt = await scApplicationDac.GetRDRPMCnt(result.RdMaxYear, orgId);
            // 取得創新提案數量
            result.RdWipCnt = await scApplicationDac.GetRDWIPCnt(result.RdMaxYear, orgId);
            return result;
        }
    }
}
