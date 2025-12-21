using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SDO.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration, IWebHostEnvironment environment)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (Exception ex)
            {
                //是否回傳詳細錯誤訊息
                //await (environment.IsDevelopment() || configuration.GetValue<bool>("ShowErrorMsg"))
                //    .IsTrue(async () => await WriteResponseRtnResultModel(context, false, ex.ToString()))
                //    .IsFalse(async () => await WriteResponseRtnResultModel(context, false, i18N.Message.R01)); 

                if(environment.IsDevelopment() || configuration.GetValue<bool>("ShowErrorMsg"))
                {
                    context.Response.StatusCode = 500;

                   // var jobj = JsonSerializer.SerializeToUtf8Bytes(ex.ToString());
                   // await context.Response.Body.WriteAsync(jobj);
                    throw new Exception(ex.ToString());
                }
                else
                {
                    throw new Exception(i18N.Message.R01);
                }
            }
        }


        private async Task WriteResponseRtnResultModel(HttpContext context, bool success, string message)
        {
            await ResponseUtil.WriteResponse(context, new RtnResultModel(success, message));
        }
    }

    public static class ExceptionMiddlewareExtension
    {
        public static IApplicationBuilder UseSDOExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
