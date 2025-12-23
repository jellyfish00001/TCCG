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
        public LoginTokenRefreshMiddleware(RequestDelegate next, ILogger<LoginTokenRefreshMiddleware> logger, IHttpContextAccessor httpContextAccessor)
        {
            this.next = next;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task InvokeAsync(HttpContext context, IUserProfile userProfile, ICache cache)
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

                _ = Task.Run(async () =>
                {
                    try
                    {
                        //將cache資料寫入cache中
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