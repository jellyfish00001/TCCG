using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Models;
using SDO.Utils;

namespace SDO.Dac
{
    public class SCAppDac : Dac, ISCAppDac
    {
        public SCAppDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)

        {

        }
        /// <summary>
        /// 取得登入使用者可用系統
        /// </summary>
        /// <returns></returns>
        public async Task<IList<SCAppModel>> GetUserApp()
        {
            return await GetUserApp(UserId);
        }
        /// <summary>
        /// 以帳號取得使用者可使用系統
        /// </summary>
        /// <param name="UserId">使用者Id</param>
        /// <returns></returns>
        public async Task<IList<SCAppModel>> GetUserApp(string userId)
        {
            string sql = @"
                select a.USR_ID,b.AP_ID,
                       b.AP_NAME,b.PRG_PATH,
                       b.AP_SORT_ORDER,DISPLAY_TYPE 
                from SCREL_AP_USRM a 
                inner join SCAPPLICATIONM b 
                  on a.AP_ID=b.AP_ID
                where USR_ID=@UserId
            ";
            return await ExecuteQueryAsync<SCAppModel>(sql, new { UserId }, SCDBKey);
        }
    }
}
