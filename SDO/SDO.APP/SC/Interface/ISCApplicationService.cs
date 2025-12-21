using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface ISCApplicationService
    {
        /// <summary>
        /// 取得Sc連結
        /// </summary>
        /// <param name="domainName">網域名稱</param>
        /// <param name="apId">系統代號</param>
        /// <returns></returns>
        Task<RtnResultModel> GetScLink(string domainName, string apId);

        /// <summary>
        /// 取得 SC 資訊
        /// </summary>
        /// <param name="orgId">組織ID</param>
        /// <returns></returns>
        Task<SCApplicationModel> GetScData(string orgId);

    }
}
