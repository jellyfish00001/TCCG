using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class FundingExecutionDac : Dac, IFundingExecutionDac
    {
        public FundingExecutionDac(
            IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 取基計畫總金額
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<int> GetFundingExecutionTOTAL(string PLANNO)
         {
            string sql = @"
                            SELECT
                                 SUM(ISNULL(PUBLICMONEY, 0) + ISNULL(FUNDMONEY, 0) + ISNULL(OTHERMONEY, 0) + ISNULL(CENTERMONEY, 0)) AS TOTAL
                            FROM PWSSDPLANMAIN
                           WHERE PLANNO = @PLANNO
                          ";
            var result = await ExecuteQueryFirstOrDefaultAsync<int>(sql, new { PLANNO }, PWSDBKey);
            return result ;
        }
    }
}
