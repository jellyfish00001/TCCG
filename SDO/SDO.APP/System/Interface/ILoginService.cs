using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface ILoginService
    {
        public string Token { get; }

        Task<BasicDataModel> LoadBasic();

        Task<string> LoadCache();

        /// <summary>
        /// 登入
        /// </summary>
        Task<ObjectResultModel<string>> Login(LoginModel login);

        /// <summary>
        /// 雙因子登入
        /// </summary>
        Task<ObjectResultModel<bool>> LoginDoubleStep(LoginModel login);
        /// <summary>
        /// 資料轉換登入
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        Task<object> SSOLogin(LoginModel login);
        Task<RtnResultModel> SetAgent(string userId);

        /// <summary>
        /// 紀錄系統登入紀錄
        /// </summary>
        /// <param name="AP_ID"></param>
        /// <returns></returns>
        Task<RtnResultModel> LoginLog(string AP_ID);

        /// <summary>
        /// 單一入口登入
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<RtnResultModel> SSOLogin(string token);

        /// <summary>
        /// 取登入者Token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<object> GetToken(LoginExChangeModel model);

        /// <summary>
        /// 取得公務雲
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<RtnResultModel> SSOCloud(string sessionId, string userId);

        /// <summary>
        /// 檢查是否可提供帳號申請功能
        /// </summary>
        /// <returns></returns>
        Task<bool> CheckAccApply();
    }
}