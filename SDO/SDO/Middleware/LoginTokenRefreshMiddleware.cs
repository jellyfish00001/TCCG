using SDO.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Models;
using System;
using System.Diagnostics.Eventing.Reader;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using SDO.Utils;
using Microsoft.Extensions.Options;
using System.Text;

namespace SDO.Middleware
{
    public class LoginTokenRefreshMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<LoginTokenRefreshMiddleware> logger;
        private readonly IHttpContextAccessor httpContextAccessor;
        private ISystemInfoService systemInfo;
        public LoginTokenRefreshMiddleware(RequestDelegate next, ILogger<LoginTokenRefreshMiddleware> logger, IHttpContextAccessor httpContextAccessor, ISystemInfoService systemInfo)
        {
            this.next = next;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.systemInfo = systemInfo;
        }

        public async Task InvokeAsync(HttpContext context, ITokenService tokenService, IUserProfile userProfile, ICache cache, IOptions<TokenSetting> tokenSettingOptions)
        {

            await next(context);

            if (!context.Response.HasStarted && userProfile.hasLogged)
            {
                string cahcheToken = httpContextAccessor.HttpContext.Request.Headers["CacheToken"].ToString();
                if (!String.IsNullOrEmpty(cahcheToken))
                {
                    var cacheString = await cache.GetStringCache(cahcheToken);
                    if (String.IsNullOrEmpty(cacheString))
                    {
                        //throw new UnauthorizedAccessException();
                        context.Response.StatusCode = 401;
                        return;
                    }
                    else
                    {
                        await cache.RefreshCache(cahcheToken);
                    }
                }

                SCUserModel user = (SCUserModel)userProfile.GetLoginUser();

                string loginToken;

                if (!tokenSettingOptions.Value.TokenRefresh && //不更新Token
                    context.Request.Headers.TryGetValue("Authorization", out StringValues loginTokenContent)) //Authorization有值
                    loginToken = loginTokenContent.ToString();
                else
                    loginToken = tokenService.GenJWTToken(user); //產出新Token

                //將Token加在Header上
                context.Response.Headers["Authorization"] = loginToken;

                string cacheToken;
                //判斷是否有cacheToken，若無則產生新的
                if (context.Request.Headers.TryGetValue("CacheToken", out StringValues cacheTokenContent))
                {
                    cacheToken = cacheTokenContent.ToString();
                }
                else
                {
                    cacheToken = Guid.NewGuid().ToString();
                }

                //打包cache資料
                IDictionary<string, string> cacheData = new Dictionary<string, string>
                {
                    { "token", loginToken },
                    { "user", user.USER_ID },
                    { "ip", user.USER_IP },
                    {"agentId",user.AGENT_ID },
                    {"ExpireTime",systemInfo.GetCacheExpireTime() }
                };

                _ = Task.Run(async () =>
                {
                    try
                    {
                        //將cache資料寫入cache中
                        await cache.SetStringCache(cacheToken.ToString(), JsonSerializer.Serialize(cacheData));
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.ToString());
                    }
                });


                //將CacheToken加在Header上
                context.Response.Headers["CacheToken"] = cacheToken;
            }
        }
    }

    public static class LoginTokenRefreshMiddlewareExtension
    {
        public static IApplicationBuilder UseLoginTokenRefresh(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LoginTokenRefreshMiddleware>();
        }
    }
}