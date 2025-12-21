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
    public class SqlLogDac : Dac, ISqlLogDac
    {
        public SqlLogDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 查詢SQL日誌
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IList<SqlTraceGridModel>> Read(GridBasicQryModel model)
        {
            string sql = @"
                SELECT COUNT(*) OVER() AS DATA_COUNT,
                       a.USER_ID,
                       b.USER_NAME,
                       a.USER_IP,
                       COMMANDTEXT,
                       PARAMETERS,
                       REQUEST_URL,
                       REPLACE(CONVERT(varchar(100),LOG_DATE, 120),'-','/') as [LOG_DATE] 
                FROM dbo.SQL_TRACE a
                        LEFT JOIN dbo.EMP_USER b ON b.USER_ID = a.USER_ID 
                WHERE FORMAT(LOG_DATE, 'yyyy-MM-dd') BETWEEN ?START_DATE? AND ?END_DATE?
                ORDER BY a.LOG_DATE DESC 
                    OFFSET ?PAGE_NO? ROWS
	                FETCH NEXT ?PAGE_SIZE? ROWS ONLY";
            return await ExecuteQueryAsync<SqlTraceGridModel>(sql, model, trace: false);
        }
    }
}
