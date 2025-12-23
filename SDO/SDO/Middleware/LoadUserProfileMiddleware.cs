using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SDO.Dac;
using SDO.Models;
using SDO.Services;
using SDO.Utils;
using System;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
namespace SDO.Middleware
{
    public class LoadUserProfileMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ICache cache;
        private readonly ILogger<AccessMiddleware> logger;

        public LoadUserProfileMiddleware(RequestDelegate next, ICache cache, ILogger<AccessMiddleware> logger)
        {
            this.next = next;
            this.cache = cache;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext, IUserProfile userProfile)
        {
            RouteData routeData = httpContext.GetRouteData();
            string controllerName = routeData.Values["controller"]?.ToString();
            string actionName = routeData.Values["action"]?.ToString();
            bool _allowAnonymous=false;
            if (httpContext.User.Identity.IsAuthenticated)
            {
                if (!string.IsNullOrWhiteSpace(controllerName))
                {
                    _allowAnonymous = allowAnonymous(controllerName, actionName);
                }

                var CacheToken = httpContext.Request.Headers["CacheToken"].ToString();

                if (!string.IsNullOrEmpty(CacheToken))
                {
                    var tokenVal =await cache.GetStringCache(CacheToken);
                    //如果不是匿名方法，且Cache不存在
                    if (string.IsNullOrEmpty(tokenVal) && !_allowAnonymous)
                    {
                        httpContext.Response.StatusCode = 401;
                        userProfile.Logout();
                        return;
                    }
                }
                else
                {
                    if (!_allowAnonymous)
                    {
                        httpContext.Response.StatusCode = 401;
                        userProfile.Logout();
                        return;
                    }
                    
                }
            }

            await next.Invoke(httpContext);
        }
        private bool allowAnonymous(string controllerName, string actionName)
        {
            if (string.IsNullOrWhiteSpace(controllerName))
                return false;

            Type type = Type.GetType($"SDO.Controllers.{controllerName}Controller");

            if (Attribute.IsDefined(type, typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute)))
                return true;

            if (Attribute.IsDefined(type, typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute)))
                return false;
            try
            {
                MethodInfo action = type.GetMethod(actionName);
                return Attribute.IsDefined(action, typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute));
            }
            catch (AmbiguousMatchException)
            {
                logger.LogError($"Controller: {controllerName} 重複ActionName: {actionName}");
                throw;
            }
        }

    }



    public static class LoadUserProfileMiddlewareExtension
    {
        public static IApplicationBuilder UseLoadUserProfile(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LoadUserProfileMiddleware>();
        }
    }
}
