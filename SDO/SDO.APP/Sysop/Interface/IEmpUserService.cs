using SDO.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SDO.Services
{
    public interface IEmpUserService
    {
        /// <summary>
        /// 查詢使用者資料
        /// </summary>
        /// <param name="id"></param>
        /// <param name="delFlg">是否停用</param>
        /// <returns></returns>
        Task<UserDataModel> GetUserById(string id, bool delFlg = false);

        /// <summary>
        /// 查詢使用者 By 單位ID
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        Task<IList<UserDataModel>> GetUserByOrg(string orgId = "");

        /// <summary>
        /// 根據登入密碼正確與否及登入時機更新EMP_USER
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="login">登入狀態，false:密碼錯誤、true:密碼正確</param>
        /// <param name="flag">登入時機，false:未鎖定、true:鎖定時間到期</param>
        Task UpdateLoginInfo(string userId, bool login, bool flag = false);

        Task<string> CheckPolicy(UserDataModel model, bool IsCreate, string PolicyID = "SDO");
        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="id">使用者帳號</param>
        /// <param name="pwd">使用者密碼</param>
        /// <returns>使用者資訊</returns>
        Task<UserDataModel> Login(LoginModel model);
        /// <summary>
        /// 驗證登入資訊
        /// </summary>
        /// <param name="Id">使用者帳號</param>
        /// <param name="PD">使用者密碼</param>
        /// <param name="Sys">系統代碼</param>
        /// <returns></returns>
        Task<(UserDataModel user, bool success)> GetLogInData(string Id, string PD, string Sys);
        Task<IList<UserDataModel>> Read(EmpUserReadModel model);
        Task<IList<UserDataModel>> GetUserByIds(string Ids);
        Task<RtnResultModel> Create(EmpUserModel createModel);
        Task<RtnResultModel> Update(EmpUserModel model);
        Task<RtnResultModel> Delete(string userId, string del_reason);
        Task<IList<SCUserModel>> ReadUserAgent();
    }
}