using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface ISCRegisterService
    {
        /// <summary>
        /// 取得SC密碼規則
        /// </summary>
        /// <returns></returns>
        Task<SCPolicyModel> GetSCPolicy();

        /// <summary>
        /// 密碼變更
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<RtnResultModel> ResetPW(SCRefreshModel model);

        /// <summary>
        /// 忘記密碼
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<RtnResultModel> ForgotPW(SCQuestionModel model);
    }
}
