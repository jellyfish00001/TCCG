using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace SDO.Dac
{
    public interface ISCUserDac : IDac
    {
        /// <summary>
        /// 檢查使用者是否存在
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="delFlg"></param>
        /// <returns></returns>
        Task<bool> CheckExists(string userId, bool delFlg);

        /// <summary>
        /// 檢查帳戶是否存在
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        Task<bool> CheckExists(string userId, string userName, string userEmail);

        /// <summary>
        /// 取得使用者資料
        /// </summary>
        /// <param name="id"></param>
        /// <param name="delFlg"></param>
        /// <returns></returns>
        Task<SCUserModel> GetUserById(string id, bool delFlg = false);

        Task<IList<SCUser>> GetUserByIds(IEnumerable<string> ids);

        /// <summary>
        /// 取得單位下使用者清單
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        Task<IList<SCUser>> GetUserByOrg(string orgId = "");

        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="login"></param>
        /// <param name="isSSOLogin">是否是單一入口登入</param>
        /// <returns></returns>
        Task<SCUserModel> Login(LoginModel login, bool isSSOLogin = false);

        Task<IList<SCUser>> Read(EmpUserReadModel model);

        Task UpdateLoginInfo(string userId, bool login, int conFault = 0, int nonconFault = 0, bool flag = false);

        /// <summary>
        /// 更新密碼
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userPd"></param>
        /// <param name="needChanPswd"></param>
        /// <returns></returns>
        Task UpdatePassword(string userId, string userPd, string needChanPswd = "N");

        Task<SCPolicyModel> GetSCPolicy();

        /// <summary>
        /// 取得密碼變更記錄檔清單
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="topCnt">筆數</param>
        /// <returns></returns>
        Task<IList<SCPswdHismModel>> GetPswdHishs(string userId, int topCnt);

        /// <summary>
        /// 檢查 token 是否有對應到 SCSESSIONM.SESSION_ID  
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<string> CheckToken(string token);
    }
}