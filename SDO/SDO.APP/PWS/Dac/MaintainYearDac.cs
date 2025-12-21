using Microsoft.AspNetCore.Http;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;

namespace SDO.Dac
{
    public class MaintainYearDac : Dac, IMaintainYearDac
    {
        public MaintainYearDac(
            IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 取計畫年度
        /// </summary>
        /// <returns></returns>
        public async Task<List<MaintainYearModel>> GetMaintainYear()
        {
            string sql = @"
                           SELECT DISTINCT PLANYEAR
                             FROM PWSSDYEARSET (NOLOCK)
                            ORDER BY PLANYEAR; 
                          ";

            var result = (await ExecuteQueryAsync<MaintainYearModel>(sql, null, PWSDBKey)).ToList();
            return result;
        }

        /// <summary>
        /// 更新計畫年度
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task SetMaintainYear(List<MaintainYearModel> models)
        {
            string insertSql = $@"
                                INSERT INTO PWSSDYEARSET
                                (
                                PLANYEAR,
                                CRT_USER,
                                CRT_DATE
                                )
                              VALUES
                                (
                                @PLANYEAR,
                                @CRT_USER,
                                {DTNow}
                                )
                                ";
            await ExecuteCommandAsync(insertSql, models, PWSDBKey);
        }

    }
}
