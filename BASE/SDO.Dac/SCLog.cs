using Microsoft.AspNetCore.Http;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class SCLog : ISqlTrace
    {
        private readonly ITraceDac traceDac;
        protected HttpContext httpContext => httpContextAccessor?.HttpContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        private IList<SCLogModel> traceList = new List<SCLogModel>();

        public SCLog(ITraceDac traceDac, IHttpContextAccessor httpContextAccessor)
        {
            this.traceDac = traceDac;
            this.httpContextAccessor = httpContextAccessor;
        }
        public SCLog(ITraceDac traceDac)
        {
            this.traceDac = traceDac;
        }

        public void SetTrace(string sql, object param, IUserData userInfo)
        {
            string path = "";
            if (httpContext != null)
                path = $"{httpContext?.Request.PathBase}{httpContext?.Request.Path}";

            AddLog(new SCLogModel()
            {
                USR_ID = userInfo.USER_ID,
                FUN_ITEM_ID = path,
                MSG_DETAIL = sql,
                LOG_TYPE = "2", //2資訊LOG(Information)
                LOG_KIND = "1", //1一般性(ACTIVITY LOG)
                LOG_TYPE2 = "4", //4資料處理或作業
                MSG_Value = JsonSerializer.Serialize(param),
                USR_IP = userInfo.USER_IP
            });
        }

        public void AddLog(object sqlTrace)
        {
            SCLogModel model = (SCLogModel)sqlTrace;
            //USR_ID 或 USR_IP 為空則不紀錄
            if (string.IsNullOrWhiteSpace(model.USR_ID) || string.IsNullOrWhiteSpace(model.USR_IP))
                return;
            lock (traceList)
            {
                if (string.IsNullOrEmpty(model.AP_ID))
                {
                    model.AP_ID = "IPC3";
                }
                traceList.Add(model);
            }
        }

        public void AddLog(string sql, object param)
        {
            lock (traceList)
            {
                traceList.Add(new SCLogModel()
                {
                    USR_ID = httpContextAccessor.HttpContext.User.Identity.Name,
                    USR_IP = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                    MSG_CONTENT = httpContextAccessor.HttpContext.Request.Path,
                    MSG_DETAIL = sql,
                    MSG_Value = JsonSerializer.Serialize(param)
                });
            }
        }

        public async Task WriteLog()
        {
            string sql = $@"
                INSERT INTO SCLOGM
                    (USR_COMP_ID
                    ,USR_ID
                    ,AGENT_USR_ID
                    ,AP_ID
                    ,FUN_ITEM_ID
                    ,MSG_ID
                    ,MSG_CONTENT
                    ,MSG_DETAIL
                    ,LOG_TIME
                    ,LOG_TYPE
                    ,LOG_KIND
                    ,LOG_TYPE2
                    ,PRT_FLAG
                    ,MSG_Value
                    ,USR_IP)
                VALUES
                    ('GSS'
                    ,@USR_ID
                    ,@AGENT_USR_ID
                    ,@AP_ID
                    ,@FUN_ITEM_ID
                    ,'system'
                    ,@MSG_CONTENT
                    ,@MSG_DETAIL
                    ,GETDATE()
                    ,@LOG_TYPE
                    ,@LOG_KIND
                    ,@LOG_TYPE2
                    ,@PRT_FLAG
                    ,@MSG_Value
                    ,@USR_IP) ";
            await traceDac.ExecuteCommandAsync(sql, traceList, "SCDBConnection");
            traceList.Clear();
        }
    }
}
