using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
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
    public class RequestMiddleWare
    {
        private readonly RequestDelegate next;
        public RequestMiddleWare(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAccessService accessService, IUserProfile userProfile)
        {
            var ReqContentType = context.Request.Headers["Content-Type"].ToString();
            if (ReqContentType.Contains("multipart/form-data"))
            {
                await next.Invoke(context);
            }
            else
            {
                context.Request.EnableBuffering();
                var stream = context.Request.Body;
                //複製並替換RequestBody物件
                using (MemoryStream newRequestStream = new MemoryStream())
                using (var reader = new StreamReader(stream))
                {
                    var requestBodyAsString = await reader.ReadToEndAsync();

                    if (stream.CanSeek)
                        stream.Seek(0, SeekOrigin.Begin);
                    await stream.CopyToAsync(newRequestStream);
                    newRequestStream.Seek(0, SeekOrigin.Begin);
                    context.Request.Body = newRequestStream;
                    await next.Invoke(context);
                }
            }
        }
    }
    public static class RequestMiddleWareExtension
    {
        public static IApplicationBuilder UseRequestBody(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestMiddleWare>();
        }
    }
}
