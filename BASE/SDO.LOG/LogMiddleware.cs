using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.IO;
using Microsoft.Extensions.Logging;
using SDO.Services;
using SDO.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using SDO.Utils;
using SDO.Dac;
using SDO.LOG.Models;

namespace SDO.LOG.Middleware
{
    public class LogMiddleware : Service
    {
        private readonly RequestDelegate next;
        private readonly ILogger<LogMiddleware> logger;
        private readonly IUserProfile userProfile;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly ISCUserDac scUserDac;
        private readonly IConfiguration Configuration;
        public LogMiddleware(RequestDelegate next, ILogger<LogMiddleware> logger, IUserProfile userProfile, IWebHostEnvironment webHostEnvironment,
            ISCUserDac scUserDac, IConfiguration Configuration)
        {
            this.next = next;
            this.logger = logger;
            this.userProfile = userProfile;
            this.webHostEnvironment = webHostEnvironment;
            this.scUserDac = scUserDac;
            this.Configuration = Configuration;
        }

        public async Task InvokeAsync(HttpContext context, ISqlTrace sqlTrace, IApiTrace apiTraceService)
        {

            try
            {
                var ComputerIP  = context.Request.Headers["ClientIP"].FirstOrDefault()==null?
                    context.Connection.RemoteIpAddress.ToString(): context.Request.Headers["ClientIP"].FirstOrDefault();
                string ComputerName = "";// System.Net.Dns.GetHostEntry(ComputerIP).HostName; //電腦名稱
                string UserName = "";
                string Uid = "";
                ApiTraceModel apiTrace = new ApiTraceModel
                {
                    REQUEST_URL = $"{context.Request.PathBase.Value}{context.Request.Path}",
                    REQUEST_HEADER = JsonSerializer.Serialize(context.Request.Headers.ToDictionary(h => h.Key, h => h.Value)),
                    REQUEST_TYPE = context.Request.Method,
                    IP = ComputerIP
                };
                //若包含Form則讀取Form 否則讀取Body
                if (context.Request.HasFormContentType)
                {
                    apiTrace.REQUEST_BODY = JsonSerializer.Serialize((await context.Request.ReadFormAsync()).ToDictionary(f => f.Key, f => f.Value));
                }
                else
                {
                    context.Request.Body.Seek(0, SeekOrigin.Begin);
                    // 確保 HTTP Request 可以多次讀取
                    apiTrace.REQUEST_BODY = await StreamToString(context.Request.Body, context.Request.Body.Length);
                    //讀完重置index
                    context.Request.Body.Seek(0, SeekOrigin.Begin);
                }

                //將Response.Body(Stream)儲存起來
                Stream oriResponseBodyStream = context.Response.Body;
                try
                {
                    using (MemoryStream newResponseBodyStream = new MemoryStream())
                    {
                        //因Response.Body(Stream)不可讀取，故將其替還為可以讀取的Stream
                        context.Response.Body = newResponseBodyStream;
                        // Call the next delegate/middleware in the pipeline
                        await next(context);

                        apiTrace.RESPONSE_CODE = context.Response.StatusCode;
                        apiTrace.RESPONSE_HEADER = JsonSerializer.Serialize(context.Response.Headers.ToDictionary(h => h.Key, h => h.Value));

                        //將Stream Position指回開頭準備讀取
                        newResponseBodyStream.Seek(0, SeekOrigin.Begin);
                        //Stream To String
                        apiTrace.RESPONSE_BODY = await StreamToString(newResponseBodyStream, Convert.ToInt32(newResponseBodyStream.Length));
                        //將Stream Position指回開頭準備讀取
                        newResponseBodyStream.Seek(0, SeekOrigin.Begin);
                        //Copy new stream to origin stream
                        await newResponseBodyStream.CopyToAsync(oriResponseBodyStream);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString() + ex.StackTrace);
                    throw;
                }
                finally
                {
                    //將Response.Body替換回原本的Stream
                    context.Response.Body = oriResponseBodyStream;
                }

                if (context.User.Identity != null && !string.IsNullOrEmpty(context.User.Identity.Name))
                {
                    Uid = context.User.Identity.Name;
                    var user = await scUserDac.GetUserById(Uid, false);
                    UserName = user.USER_NAME;
                }
                //Api Trace
                if (apiTrace.RESPONSE_CODE != 404)
                {
                    //將routeData另存避免非同步時Request Dispose
                    IDictionary<string, object> routeData = context.Request.RouteValues.ToDictionary(r => r.Key, r => r.Value);
                    _ = Task.Run(async () => //不等待結果
                    {
                        await EncodeUserPd(apiTraceService, routeData, apiTrace);
                        routeData.TryGetValue("controller", out object controllerObj);
                        routeData.TryGetValue("action", out object actionObj);
                        Dictionary<string, object> RequestBody = new Dictionary<string, object>();
                        if (ParseUtil.IsValidJson(apiTrace.REQUEST_BODY))
                        {
                            RequestBody = string.IsNullOrEmpty(apiTrace.REQUEST_BODY) ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(apiTrace.REQUEST_BODY);
                        }
                        else
                            RequestBody.Add("Body", apiTrace.REQUEST_BODY);
                        try
                        {
                            var EncryptCols = Configuration.GetValue<string>("EncryptCols");
                            if (EncryptCols!=null &&EncryptCols.Any() && RequestBody.Any(x => EncryptCols.Contains(x.Key)))
                            {
                                foreach (var item in RequestBody.Where(x => EncryptCols.Contains(x.Key)))
                                {
                                    RequestBody[item.Key] = await apiTraceService.MD5Encode(item.Value.ToString());
                                }
                                apiTrace.REQUEST_BODY = JsonSerializer.Serialize(RequestBody);
                            }
                            // logger.LogInformation("")
                            //await apiTraceService.AddTrace(apiTrace);
                            //await SetLog(new LogModel
                            //{
                            //    LogTime = DateTime.UtcNow.AddHours(8),
                            //    URL = apiTrace.REQUEST_URL,
                            //    RequestBody = RequestBody,
                            //    User_Id = Uid,
                            //    User_Ip = ComputerIP,
                            //    User_Macheine = ComputerName,
                            //    Controller = controllerObj.ToString(),
                            //    Action = actionObj.ToString(),
                            //    User_Name = UserName
                            //});
                        }
                        catch (Exception ex)
                        {
                            logger.LogInformation("url:" + apiTrace.REQUEST_URL);
                            logger.LogError(ex.ToString() + ex.StackTrace);
                        }
                    });
                }

                //SqlTrace
                _ = Task.Run(async () => //不等待結果
                {
                    try
                    {
                        await sqlTrace.WriteLog();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.ToString() + ex.StackTrace);
                    }
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString() + ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// 若Request內容含有密碼則進行Encode
        /// </summary>
        /// <param name="apiTraceService"></param>
        /// <param name="routeData"></param>
        /// <param name="apiTrace"></param>
        /// <returns></returns>
        private async Task EncodeUserPd(IApiTrace apiTraceService, IDictionary<string, object> routeData, ApiTraceModel apiTrace)
        {
            routeData.TryGetValue("controller", out object controllerObj);
            routeData.TryGetValue("action", out object actionObj);
            if (controllerObj != null && controllerObj.ToString().ToUpper().Contains("LOGIN") && actionObj != null && actionObj.ToString() == "Post")
            {
                LoginData loginData = JsonSerializer.Deserialize<LoginData>(apiTrace.REQUEST_BODY);
                loginData.USER_PD = await apiTraceService.MD5Encode(loginData.USER_PD);
                apiTrace.REQUEST_BODY = JsonSerializer.Serialize(loginData);
            }
        }

        private class LoginData
        {
            public string USER_ID { get; set; }
            public string USER_PD { get; set; }
        }

        private async Task<string> StreamToString(Stream stream, long contentLength)
        {
            if (contentLength == 0)
                return string.Empty;
            Memory<byte> buffer = new byte[contentLength < 4096 ? contentLength : 4096];
            StringBuilder stringBuilder = new StringBuilder();

            while (true)
            {
                int readBytes = await stream.ReadAsync(buffer);

                stringBuilder.Append(Encoding.UTF8.GetString(buffer.Slice(0, readBytes).Span));
                if (readBytes == 0 || readBytes < buffer.Length)
                    break;
            }

            return stringBuilder.ToString();

        }

        private async Task<bool> SetLog(LogModel log)
        {
            var pathstructure = webHostEnvironment.ContentRootPath.Split('\\').ToList();
            pathstructure.Remove(pathstructure.LastOrDefault());
            string WEBFolder = string.Join("\\", pathstructure.ToArray());
            string path = @$"{WEBFolder}\logs\apIlog\";
            string logFile = DateTime.Now.ToString("yyyyMMdd") + ".log";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (!File.Exists(@$"{path}\{logFile}"))
                await File.WriteAllLinesAsync(@$"{path}\{logFile}", new string[] { JsonSerialize(log).Replace("\\u0022", "\"") });
            else
                await File.AppendAllLinesAsync(@$"{path}\{logFile}", new string[] { JsonSerialize(log).Replace("\\u0022", "\"") });
            return true;
        }
    }

    public static class LogMiddlewareExtension
    {
        public static IApplicationBuilder UseLog(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LogMiddleware>();
        }
    }
}
