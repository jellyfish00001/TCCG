using Microsoft.AspNetCore.Http;
using SDO.Dac;
using SDO.Models;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class SqlTrace : ISqlTrace
    {
        private readonly ITraceDac traceDac;
        protected HttpContext httpContext => httpContextAccessor?.HttpContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        private IList<SqlTraceModel> traceList = new List<SqlTraceModel>();

        public SqlTrace(ITraceDac traceDac, IHttpContextAccessor httpContextAccessor)
        {
            this.traceDac = traceDac;
            this.httpContextAccessor = httpContextAccessor;
        }

        public SqlTrace(ITraceDac traceDac)
        {
            this.traceDac = traceDac;
        }

        public void SetTrace(string sql, object param, IUserData userInfo)
        {
            string path = "";
            if (httpContext != null)
                path = $"{httpContext?.Request.PathBase}/{httpContext?.Request.Path}";

            AddLog(new SqlTraceModel()
            {
                USER_ID = userInfo.USER_ID,
                USER_IP = userInfo.USER_IP,
                COMMANDTEXT = sql,
                PARAMETERS = JsonSerializer.Serialize(param),
                REQUEST_URL = path
            });
        }

        public void AddLog(object sqlTrace)
        {
            SqlTraceModel model = (SqlTraceModel)sqlTrace;
            //USER_ID 或 USER_IP 為空則不紀錄
            if (string.IsNullOrWhiteSpace(model.USER_ID) || string.IsNullOrWhiteSpace(model.USER_IP))
                return;
            lock (traceList)
            {
                traceList.Add(model);
            }
        }

        public void AddLog(string sql, object param)
        {
            lock (traceList)
            {
                traceList.Add(new SqlTraceModel()
                {
                    USER_ID = httpContextAccessor.HttpContext.User.Identity.Name,
                    USER_IP = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                    COMMANDTEXT = sql,
                    PARAMETERS = JsonSerializer.Serialize(param),
                    REQUEST_URL = httpContextAccessor.HttpContext.Request.Path
                });
            }
        }

        public async Task WriteLog()
        {
            string sql = @"
                INSERT INTO dbo.SQL_TRACE(
                    USER_ID,
                    USER_IP,
                    COMMANDTEXT,
                    PARAMETERS,
                    REQUEST_URL)
                VALUES(
                    ?USER_ID?,
                    ?USER_IP?,
                    ?COMMANDTEXT?,
                    ?PARAMETERS?,
                    ?REQUEST_URL?)
                ";
            await traceDac.ExecuteCommandAsync(sql, traceList);
            traceList.Clear();
        }
    }
}
