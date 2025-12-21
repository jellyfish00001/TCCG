using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class ApiLogDac : Dac, IApiLogDac
    {
        public ApiLogDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 讀取ApiLog
        /// </summary>
        /// <returns></returns>
        public async Task<IList<ApiTraceModel>> Read()
        {
            string sql = @"
                SELECT 
                    [SID]
                    ,[IP]
                    ,[REQUEST_HEADER]
                    ,[REQUEST_BODY]
                    ,[REQUEST_URL]
                    ,[REQUEST_TYPE]
                    ,[RESPONSE_HEADER]
                    ,[RESPONSE_BODY]
                    ,[RESPONSE_CODE]
                    ,REPLACE(CONVERT(VARCHAR(100),LOG_DATE, 120),'-','/') as [LOG_DATE] 
                FROM [API_TRACE]";
            return await ExecuteQueryAsync<ApiTraceModel>(sql);
        }


        /// <summary>
        /// 根據日期範圍讀取ApiLog
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IList<ApiTraceModel>> ReadByDate(GridBasicQryModel model)
        {
            string sql = @"
                SELECT 
                    COUNT(*) OVER() AS DATA_COUNT 
                    ,[SID]
                    ,[IP]
                    ,[REQUEST_HEADER]
                    ,[REQUEST_BODY]
                    ,[REQUEST_URL]
                    ,[REQUEST_TYPE]
                    ,[RESPONSE_HEADER]
                    ,[RESPONSE_BODY]
                    ,[RESPONSE_CODE]
                    ,REPLACE(CONVERT(VARCHAR(100),LOG_DATE, 120),'-','/') as [LOG_DATE] 
                FROM [API_TRACE]
                WHERE FORMAT(LOG_DATE, 'yyyy-MM-dd') BETWEEN ?START_DATE? AND ?END_DATE?";
            sql += @"ORDER BY LOG_DATE DESC" + FetchNext();
            return await ExecuteQueryAsync<ApiTraceModel>(sql, model);
        }

        /// <summary>
        /// 過濾需要哪幾筆資料
        /// </summary>
        /// <returns></returns>
        private string FetchNext()
        {
            return @"
                OFFSET ?PAGE_NO? ROWS
	            FETCH NEXT ?PAGE_SIZE? ROWS ONLY";
        }
    }
}
