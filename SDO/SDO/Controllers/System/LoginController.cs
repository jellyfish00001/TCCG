using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SDO.Services;
using SDO.Models;
using Microsoft.AspNetCore.Authorization;
using SDO.Attributes;
using SDO.Utils;
using System.Collections.Generic;
using System.Web;
using System.IO;
using Microsoft.Extensions.Logging;

namespace SDO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService loginService;
        private readonly ICaptcha captcha;
        private readonly ICache cache;
        private readonly ISCApplicationService scAppService;
        private readonly ISCRegisterService scRegisterService;
        private readonly ILogger<LoginController> logger;

        public LoginController(ILoginService loginService, 
            ICaptcha captcha,
            ICache cache, 
            ISCApplicationService scAppService, 
            ISCRegisterService scRegisterService, 
            ILogger<LoginController> logger)
        {
            this.loginService = loginService;
            this.captcha = captcha;
            this.cache = cache;
            this.scAppService = scAppService;
            this.scRegisterService = scRegisterService;
            this.logger = logger;
        }

        // POST: api/Login
        [AllowAnonymous]
        [HttpPost]
        public async Task<ObjectResultModel<string>> Post(LoginModel model)
        {
            return await loginService.Login(model);
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<ObjectResultModel<bool>> LoginDoubleStep(LoginModel model)
        {
            var result = await loginService.LoginDoubleStep(model);

            return result;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<string> LoadCache()
        {
            return await loginService.LoadCache();
        }
        [AllowAnonymous]
        [HttpGet("[action]")]
        public CaptchaModel LoadCapcha()
        {
            return captcha.GenerateValidateText();
        }
        [AuthorizeLogin]
        [HttpGet("[action]")]
        public async Task<BasicDataModel> LoadBasic()
        {
            return await loginService.LoadBasic();
        }

        //切換為代理人
        [AuthorizeLogin]
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SetAgent([FromBody] string userId)
        {
            return await loginService.SetAgent(userId);
        }

        /// <summary>
        /// 讀取登入log
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task DelCache([FromBody] string key)
        {
            await cache.DelCache(key);
        }

        /// <summary>
        /// 取得Sc連結
        /// </summary>
        /// <param name="domainName">網域名稱</param>
        /// <param name="apId">系統代號</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("[action]/{domainName}/{apId}")]
        public async Task<RtnResultModel> GetScLink(string domainName, string apId)
        {
            return await scAppService.GetScLink(domainName, apId);
        }

        /// <summary>
        /// 取得SC密碼規則
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<SCPolicyModel> GetSCPolicy()
        {
            return await scRegisterService.GetSCPolicy();
        }

        /// <summary>
        /// 密碼變更
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<RtnResultModel> ResetPW(SCRefreshModel model)
        {
            return await scRegisterService.ResetPW(model);
        }

        /// <summary>
        /// 忘記密碼
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<RtnResultModel> ForgotPW(SCQuestionModel model)
        {
            return await scRegisterService.ForgotPW(model);
        }

        /// <summary>
        /// 紀錄系統登入紀錄
        /// </summary>
        /// <param name="AP_ID"></param>
        /// <returns></returns>
        [Authorize(Roles = "handUser")]
        [HttpPost("[action]")]
        public async Task<RtnResultModel> LoginLog([FromBody] string AP_ID)
        {
            return await loginService.LoginLog(AP_ID);
        }

        /// <summary>
        /// 單一入口登入
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SSOLogin([FromBody] string token)
        {
            return await loginService.SSOLogin(HttpUtility.HtmlEncode(token));
        }

        /// <summary>
        /// 取登入者Token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<object> GetToken(LoginExChangeModel model)
        {
            return await loginService.GetToken(model);
        }

        /// <summary>
        /// 公務雲登入
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SSOCloud([FromForm] string sessionId, [FromForm] string userId)
        {
            logger.LogInformation($"SSOCloud sessionId:{sessionId} userId:{userId}");

            ldapService.LdapServicePortTypeClient ldapService = new ldapService.LdapServicePortTypeClient();

            ldapService.Open();
            logger.LogInformation($"SSOCloud Open ldapServiceState:{ldapService.State}");

            string ssoUserId = ldapService.verifySessionId(sessionId);
            logger.LogInformation($"SSOCloud ssoUserId:{ssoUserId}");
            ldapService.Close();

            if (string.IsNullOrEmpty(ssoUserId) || !userId.Equals(ssoUserId))
            {
                return new RtnResultModel(false);
            }

            return await loginService.SSOCloud(sessionId, userId);
        }

        /// <summary>
        /// 檢查是否可提供帳號申請功能
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task<bool> CheckAccApply()
        {
            return await loginService.CheckAccApply();
        }
    }
}
