using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SDO.Models;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using SDO.Utils;
using SDO.Dac;
using System.Security;
using SDO.CryptSet;
using System.Web;
using System.IO;

namespace SDO.Services
{
    public class LoginService : Service, ILoginService
    {
        private readonly IEmpUserDac empUserDac;
        private readonly IEmpOrgDac empOrgDac;
        private readonly IScPolicyDac scPolicyDac;
        private readonly IUserProfile userProfile;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ISysParam sysParam;
        private readonly ISetParamService setParam;
        private readonly IMailSetService Mail;
        private readonly ICache cache;
        private readonly ICaptcha captcha;
        private readonly ITokenDac tokenDac;
        private readonly IDecryptService decrypt;
        private readonly IEncryptService encrypt;
        private readonly string userIP;

        private delegate Task<UserDataModel> UserLoader(string id, bool del = false);
        private UserLoader GetUser;

        public string Token { get { return _Token; } }
        protected string _Token;
        public LoginService(IEmpUserDac empUserDac,
            IScPolicyDac scPolicyDac,
            IEmpOrgDac empOrgDac,
            IUserProfile userProfile,
            ISysParam sysParam,
            ISetParamService setParam,
            IHttpContextAccessor httpContextAccessor,
            ICache cache,
            ICaptcha captcha,
            ITokenDac tokenDac,
            IEncryptService encrypt,
            IDecryptService decrypt,
            IMailSetService Mail)
        {
            this.empUserDac = empUserDac;
            this.scPolicyDac = scPolicyDac;
            this.empOrgDac = empOrgDac;
            this.userProfile = userProfile;
            this.httpContextAccessor = httpContextAccessor;
            this.cache = cache;
            this.sysParam = sysParam;
            this.captcha = captcha;
            this.tokenDac = tokenDac;
            this.encrypt = encrypt;
            this.decrypt = decrypt;
            this.setParam = setParam;
            this.Mail = Mail;
            userIP = httpContextAccessor?.HttpContext.Connection.RemoteIpAddress.ToString();
        }

        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public async Task<ObjectResultModel<string>> Login(LoginModel login)
        {
            int authType = int.Parse((await sysParam.GetSysParam("SystemConfig", "AuthType")).SET_VALUE);
            switch (authType)
            {
                //本地驗證
                case 1:
                    return await LoginNormal(login, "SDO");
            }

            return new ObjectResultModel<string>(false, i18N.Message.R02);
        }

        ///// <summary>
        ///// 雙因子登入
        ///// </summary>
        ///// <param name="login"></param>
        ///// <returns></returns>
        public virtual async Task<ObjectResultModel<bool>> LoginDoubleStep(LoginModel login)
        {
            ObjectResultModel<bool> loginResult = LoginChangeResult(false, i18N.Message.R02, false);
            //判斷驗證階段
            if (login.isPass2Auth)//雙因子第二階段
            {
                return await CheckToken(login.TOKEN, "SDO");
            }
            else
            {
                return await DblLoginCheckSnd(login, "SDO");
            }
        }


        /// <summary>
        /// 雙因子第一段驗證
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public async virtual Task<ObjectResultModel<bool>> DblLoginCheckSnd(LoginModel login, string LoginSys)
        {
            string msgId = "LOGFAIL";
            string msgContent = "登入失敗";
            string msgDetail = "帳號或密碼輸入錯誤";
            ObjectResultModel<bool> loginResult = LoginChangeResult(false, i18N.Message.R02, false);
            //使用者帳號驗證
            var user = await GetLoginUser(login);
            if (user.userData == null || user.userData == default(UserDataModel))
            {
                return loginResult;
            }
            //帳號有效性驗證
            var result = await LoginCheck(user.userData, user.Uid, user.LoginSYS);
            if (user.Uid == null)
            {
                await empUserDac.InsertLoginLog(new()
                {
                    USER_ID = login.USER_ID,
                    USER_IP = userIP,
                    LOG_AP = LoginSys,
                    MSG_ID = msgId,
                    MSG_CONTENT = msgContent,
                    MSG_DETAIL = msgDetail
                });
            }
            if (user.userData.IS_ENABLED == "N")
            {
                return LoginChangeResult(false, "此帳號已停用", false);
            }
            if (result.success)
            {
                _Token = await createToken(user.Uid, "L");
                // PMO寄發雙因子認證信件
                await Set2AuthTemplateSend("PMO", _Token, user.userData);
                result.msg = "已將登入連結寄發至您的信箱，請於信件中點選連結登入系統。";
            }
            return LoginChangeResult(result.success, result.msg, true);
        }
        /// <summary>
        /// 驗證token
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        public async virtual Task<ObjectResultModel<bool>> CheckToken(string Token, string logAP)
        {
            Token = decrypt.AES256(Token).decryptedString;
            var TokenMod = await tokenDac.AuthenticationToken(Token, userIP, logAP);
            //讀取Token
            if (TokenMod != null)
            {
                //認證成功將該筆停用
                await tokenDac.Delete(TokenMod);
                var user = await empUserDac.GetUserById(TokenMod.USER_ID);
                userProfile.SetLoginUser(user);

                LoginLogModel loginLogModel = new LoginLogModel()
                {
                    USER_ID = user.USER_ID,
                    USER_IP = userIP,
                    LOG_AP = logAP,
                    MSG_CONTENT = "登入成功",
                    MSG_DETAIL = "登入成功"
                };
                //新增loginLog
                await empUserDac.InsertLoginLog(loginLogModel);
                return LoginChangeResult(true, "雙因子登入成功", false);
            }
            else
            {
                return LoginChangeResult(false, "驗證連結已失效", false);
            }
        }

        /// <summary>
        /// 登入驗證
        /// </summary>
        /// <param name="user">使用者資料</param>
        /// <param name="PassedUid">驗證成功使用者ID</param>
        /// <param name="SysType">登入系統驗證ID</param>
        /// <returns>bool:成功失敗 stringL回傳訊息 </returns>
        protected virtual async Task<(bool success, string msg)> LoginCheck(UserDataModel user, string PassedUid, string SysType)
        {
            string Policy;
            // 有使用者資料且密碼驗證正確則檢查 policy
            if (!string.IsNullOrEmpty(PassedUid) && user != default(UserDataModel))
            {
                //檢查policy
                Policy = await CheckPolicy(user, true, SysType);
                if (!string.IsNullOrEmpty(Policy))
                {
                    return (false, Policy);
                }
                return (true, Policy);
            }
            else
            {
                //抓policy規則
                Policy = await CheckPolicy(user, false, SysType);
                return (false, Policy);
            }
        }

        /// <summary>
        /// 取得登入使用者資訊
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        protected virtual async Task<(UserDataModel userData, string Uid, string LoginSYS)> GetLoginUser(LoginModel login)
        {
            var user = await empUserDac.GetUserById(login.USER_ID);
            GetUser = empUserDac.GetUserById;
            var loginUserID = (await empUserDac.Login(login))?.USER_ID;
            return (user, loginUserID, "EMP_USER");
        }
        /// <summary>
        /// 產生token
        /// </summary>
        /// <param name="Uid">帳號</param>
        /// <param name="tokenType">類別 A:資料交換 D: 雙因子驗證</param>
        /// <returns></returns>
        protected async Task<string> createToken(string Uid, string tokenType)
        {
            string token = Guid.NewGuid().ToString();
            TokenModel tokenModel = await tokenDac.ReadLoginToken(Uid, tokenType);
            int tokenPeriod = int.Parse((await sysParam.GetSysParam("SystemConfig", "TokenPeriod")).SET_VALUE);
            if (tokenModel == null || tokenType == "L")
            {
                await tokenDac.Insert(new TokenModel
                {
                    TOKEN = token,
                    TOKEN_TYPE = tokenType,
                    USER_ID = Uid,
                    EXPIRE_DATE = DateTime.Now.AddMinutes(tokenPeriod),
                    CRT_USER = Uid,
                    MDF_USER = Uid
                });
            }
            else
            {
                tokenModel.TOKEN = token;
                tokenModel.EXPIRE_DATE = DateTime.Now.AddMinutes(tokenPeriod);
                tokenModel.MDF_USER = Uid;
                await tokenDac.Update(tokenModel);
            }
            return token;
        }
        /// <summary>
        /// 一般登入
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        protected virtual async Task<ObjectResultModel<string>> LoginNormal(LoginModel login, string LoginAP)
        {
            string msgId = "LOGFAIL";
            string msgContent = "登入失敗";
            string msgDetail = "帳號或密碼輸入錯誤";

            if (!await captcha.CheckCaptcha(login.Captcha, login.CaptchaEncoded))
            {
                return new ObjectResultModel<string>(false, i18N.Message.S15);
            }
            // 取得使用者資訊，判斷 LOGIN_ID 是否存在
            var loginData = await GetLoginUser(login);
            // 沒有使用者資料
            if (loginData.userData == null || loginData.userData == default(UserDataModel))
            {
                return new ObjectResultModel<string>(false, i18N.Message.R02);
            }
            // 帳號是否已啟用
            if (loginData.userData.IS_ENABLED == "N")
            {
                return new ObjectResultModel<string>
                {
                    success = false,
                    message = "此帳號尚未啟用，請問需再次寄發驗證信?",
                    data = loginData.userData.IS_ENABLED
                };
            }

            var checkResult = await LoginCheck(loginData.userData, loginData.Uid, loginData.LoginSYS);
            if (string.IsNullOrEmpty(loginData.Uid))
            {
                await empUserDac.InsertLoginLog(new()
                {
                    USER_ID = login.USER_ID,
                    USER_IP = userIP,
                    LOG_AP = LoginAP,
                    MSG_ID = msgId,
                    MSG_CONTENT = msgContent,
                    MSG_DETAIL = msgDetail
                });
                return new ObjectResultModel<string>(false, i18N.Message.R02);
            }
            // 有使用者資料且密碼驗證正確則檢查 policy
            if (checkResult.success)
            {
                userProfile.SetLoginUser(loginData.userData);

                LoginLogModel loginLogModel = new LoginLogModel()
                {
                    USER_ID = userProfile.GetLoginUser().USER_ID,
                    USER_IP = userProfile.GetLoginUser().USER_IP,
                    LOG_AP = LoginAP,
                    MSG_CONTENT = "登入成功",
                    MSG_DETAIL = "登入成功"
                };
                //新增loginLog
                await empUserDac.InsertLoginLog(loginLogModel);
            }
            return new ObjectResultModel<string>(checkResult.success, checkResult.msg);
        }

        /// <summary>
        /// 資料轉換登入
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public async Task<object> SSOLogin(LoginModel login)
        {

            // 取得使用者資訊，判斷是否存在
            GetUser = empUserDac.GetUserById;
            UserDataModel loginData = await GetUser(login.USER_ID);
            // 沒有使用者資料
            if (loginData == default(UserDataModel))
            {
                return ChangeResult(false, i18N.Message.R02);
            }

            // 判斷密碼是否正確
            UserDataModel logonUser = await empUserDac.Login(new LoginModel { USER_ID = login.USER_ID, USER_PD = login.USER_PD });
            if (logonUser == default(UserDataModel))
            {
                return ChangeResult(false, i18N.Message.R02);
            }
            // 有使用者資料且密碼驗證正確則檢查policy
            string strPolicy = await CheckPolicy(loginData, true);
            if (!string.IsNullOrEmpty(strPolicy))
            {
                return ChangeResult(false, strPolicy);
            }
            LoginLogModel loginLogModel = new LoginLogModel()
            {
                USER_ID = login.USER_ID,
                USER_IP = userIP,
                LOG_AP = "GW",
                MSG_CONTENT = "介接登入成功",
                MSG_DETAIL = "介接登入成功"
            };
            //新增loginLog
            await empUserDac.InsertLoginLog(loginLogModel);
            // 設定 userProfile
            userProfile.SetLoginUser(loginData);
            var token = await createToken(loginData.USER_ID, "A");

            return new RtnExChgResultModel(true, token);
        }

        public async Task<string> LoadCache()
        {
            string cahcheToken = httpContextAccessor.HttpContext.Request.Headers["CacheToken"].ToString();
            string cacheString = await cache.GetStringCache(cahcheToken);
            if (string.IsNullOrEmpty(cacheString))
                return string.Empty;

            IDictionary<string, string> cacheData = JsonDeserialize<Dictionary<string, string>>(cacheString);
            if (cacheData != null && cacheData.TryGetValue("token", out string loginToken) && cacheData.TryGetValue("ip", out string ip))
            {
                if (!string.IsNullOrWhiteSpace(ip) && ip.Equals(httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString()))
                    return loginToken;
            }
            return string.Empty;
        }

        [Authorize(Roles = "handUser")]
        public async Task<BasicDataModel> LoadBasic()
        {
            BasicDataModel basicData = new BasicDataModel()
            {
                USER_ID = userProfile.GetLoginUser().USER_ID,
                EMAIL = userProfile.GetLoginUser().USER_EMAIL,
            };

            basicData.ORG_ID = (await empOrgDac.ReadByUserId(basicData.USER_ID)).ORG_ID;
            return basicData;
        }

        public async Task<RtnResultModel> SetAgent(string userId)
        {
            UserDataModel loginUser = (UserDataModel)userProfile.GetLoginUser();
            //切換身分，以代理人身分登入
            UserDataModel newLoginUser = await empUserDac.GetUserById(userId);

            if (newLoginUser != default(UserDataModel))
            {
                newLoginUser.AGENT_ID = loginUser.USER_ID;
                newLoginUser.USER_NAME = string.Format("{0}({1})", newLoginUser.USER_NAME, loginUser.USER_NAME);
                //設定userProfile
                userProfile.SetLoginUser(newLoginUser);
                return ChangeResult(true);
            }
            return ChangeResult(false, i18N.Message.R01);
        }

        /// <summary>
        /// check login policy
        /// </summary>
        /// <param name="model"></param>
        /// <param name="login">登入狀態，false:密碼錯誤、true:密碼正確</param>
        /// <returns></returns>
        public async Task<string> CheckPolicy(UserDataModel model, bool login, string Table = "EMP_USER", string Policy = "SDO")
        {
            StringBuilder strPolicy = new StringBuilder("");
            //根據POLICY_ID、POLICY_COMP_ID取得POLICY
            ScPolicyModel policy = await scPolicyDac.GetPolicy(Policy, "GSS");


            //check:是否鎖定，unlock:鎖定時間是否到期
            bool check = false, unlock = false;
            if (policy != null && policy.ID_NO_CLOSE == "N")
            {
                if (policy.ID_DISABLE_IN_CONT > 0 && model.CON_FAULT >= policy.ID_DISABLE_IN_CONT)
                {
                    check = true;
                    //判斷是否已過鎖定時間
                    if (Now < model.LAST_FAILLOGIN.AddMinutes(policy.ID_DISABLE_IN_CONT_TIMES))
                    {
                        strPolicy.Append(i18N.Message.S11 + policy.ID_DISABLE_IN_CONT + i18N.Message.S13);
                    }
                    else
                    {
                        unlock = true;
                    }
                }
                if (policy.ID_DISABLE_IN_NONCONT > 0 && model.NONCON_FAULT >= policy.ID_DISABLE_IN_NONCONT)
                {
                    check = true;
                    //判斷是否已過鎖定時間
                    if (Now < model.LAST_FAILLOGIN.AddMinutes(policy.ID_DISABLE_IN_NONCONT_TIMES))
                    {
                        strPolicy.Append(i18N.Message.S12 + policy.ID_DISABLE_IN_NONCONT + i18N.Message.S13);
                    }
                    else
                    {
                        unlock = true;
                    }
                }
                //判斷狀態是否為鎖定
                if (check)
                {
                    //判斷是否已過鎖定時間
                    if (unlock)
                    {
                        await empUserDac.UpdateLoginInfo(model.USER_ID, login, flag: true, Table: Table);
                    }
                }
                else
                {
                    UserDataModel user = await GetUser(model.USER_ID);
                    await empUserDac.UpdateLoginInfo(model.USER_ID, login, user.CON_FAULT, user.NONCON_FAULT, false, Table);
                }
            }
            else
            {
                UserDataModel user = await GetUser(model.USER_ID);
                await empUserDac.UpdateLoginInfo(model.USER_ID, login, user.CON_FAULT, user.NONCON_FAULT, false, Table);
            }

            if (strPolicy.ToString() != "")
            {
                strPolicy.Append(i18N.Message.S14);
            }
            return strPolicy.ToString();
        }

        /// <summary>
        /// 讀取登入log
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<LoginLogModel>> GetLoginLog(string userId)
        {
            return await empUserDac.GetLoginLog(string.IsNullOrEmpty(userId) ? userProfile.GetLoginUser().USER_ID : userId);
        }

        /// <summary>
        /// 雙因子認證發信
        /// </summary>
        /// <param name="loginWebsite"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool> Set2AuthTemplateSend(string loginWebsite, string Token, UserDataModel user)
        {
            // 登入申請共通平台的連結
            // 註冊信有效時間(分)
            int regMailPeriod = int.Parse((await setParam.GetSysParam("SystemConfig", "RegMailPeriod")).SET_VALUE);
            // 加密
            string encodeUserId = encrypt.AES256($"{Token}").encryptedString;
            string encoded = HttpUtility.UrlEncode(encodeUserId);
            // 連結網址
            string LOGIN_URL = "";
            string frontendHost = (await setParam.GetSysParam("DOMAIN_NAME", "SDO")).SET_VALUE;
            LOGIN_URL = Path.Combine(frontendHost, $"DblAuth?Token={encoded}");

            var mailAddrs = new List<RecipientModel>
            {
                new RecipientModel { MAIL_ADDRESS = user.USER_EMAIL, MAIL_TITLE = user.USER_NAME },
            };
            MailTemplateSendModel<object> mailModel = new MailTemplateSendModel<object>()
            {
                TemplateId = "Login2Auth",
                MailAddrs = mailAddrs,
                TemplatePara = new { LOGIN_URL }
            };
            return await Mail.SetTemplateSend(mailModel);
        }

        /// <summary>
        /// 紀錄系統登入紀錄
        /// </summary>
        /// <param name="AP_ID"></param>
        /// <returns></returns>
        public Task<RtnResultModel> LoginLog(string AP_ID)
        {
            throw new NotImplementedException();
        }

        public Task<RtnResultModel> SSOLogin(string token)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetToken(LoginExChangeModel model)
        {
            throw new NotImplementedException();
        }

        public Task<RtnResultModel> SSOCloud(string sessionId, string userId)
        {
            throw new NotImplementedException();
        }

        public  Task<bool> CheckAccApply()
            => throw new NotImplementedException();
    }
}
