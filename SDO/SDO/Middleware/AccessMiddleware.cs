using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SDO.Controllers;
using SDO.Models;
using SDO.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SDO.Dac;
namespace SDO.Middleware
{
    public class AccessMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<AccessMiddleware> logger;

        public AccessMiddleware(RequestDelegate next, ILogger<AccessMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IUserProfile userProfile)
        {
            RouteData routeData = context.GetRouteData();
            string controllerName = routeData.Values["controller"]?.ToString();
            string actionName = routeData.Values["action"]?.ToString();

            if (!string.IsNullOrWhiteSpace(controllerName))
            {
                //bool check = await accessService.CheckControllerAccess(controllerName);
                bool check = userProfile.hasLogged;

                if (!check && !allowAnonymous(controllerName, actionName) //該Action是否不須驗證
                    && !(userProfile.hasLogged && allowLogin(controllerName, actionName))) //該Action使否僅驗證登入不驗證權限
                {
                    await ResponseUtil.WriteResponse(context, new AuthorizationFailureResultModel());
                    context.Response.StatusCode = 401;
                    return;
                }  
            }

            await next.Invoke(context);
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
            catch(AmbiguousMatchException)
            {
                logger.LogError($"Controller: {controllerName} 重複ActionName: {actionName}");
                throw;
            }
        }

        private bool allowLogin(string controllerName, string actionName)
        {
            if (string.IsNullOrWhiteSpace(controllerName))
                return false;

            Type type = Type.GetType($"SDO.Controllers.{controllerName}Controller");

            if (Attribute.IsDefined(type, typeof(Attributes.AuthorizeLoginAttribute)))
                return true;

            try
            {
                MethodInfo action = type.GetMethod(actionName);
                return Attribute.IsDefined(action, typeof(Attributes.AuthorizeLoginAttribute));
            }
            catch (AmbiguousMatchException)
            {
                logger.LogError($"Controller: {controllerName} 重複ActionName: {actionName}");
                throw;
            }
        }
    }

    public static class AccessMiddlewareExtension
    {
        public static IApplicationBuilder UseAccessControl(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AccessMiddleware>();
        }
    }
}
