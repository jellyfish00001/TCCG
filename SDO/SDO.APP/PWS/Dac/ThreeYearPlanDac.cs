using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class ThreeYearPlanDac : Dac, IThreeYearPlanDac
    {
        public ThreeYearPlanDac(
            IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 新增近三年相關研究
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task SaveThreeYearPlan(List<ThreeYearPlanModel> models)
        {
            string sql = $@"
                        INSERT INTO PWSSDTREEYEARPLAN 
                        (
                            PLANNO, 
                            THREEYEARID, 
                            PLANNAME, 
                            STUDYYEAR, 
                            BUDGET, 
                            SITUATIONTYPE, 
                            SITUATIONDESC,
                            CRT_USER,
                            CRT_DATE
                        ) 
                        VALUES 
                        (
                            @PLANNO, 
                            @THREEYEARID, 
                            @PLANNAME, 
                            @STUDYYEAR, 
                            @BUDGET, 
                            @SITUATIONTYPE, 
                            @SITUATIONDESC,
                            @CRT_USER,
                            {DTNow}
                        )
                        ";

            await ExecuteCommandAsync(sql, models, PWSDBKey);
        }

        /// <summary>
        /// 刪除近三年相關研究
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task DeleteThreeYearPlan(string PLANNO)
        {
            string sql = @"
                            DELETE FROM
                                PWSSDTREEYEARPLAN
                            WHERE 
                                PLANNO = @PLANNO;
                          ";

            await ExecuteCommandAsync(sql, new { PLANNO }, PWSDBKey);
        }

        /// <summary>
        /// 取得近三年相關研究
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public async Task<List<ThreeYearPlanModel>> GetThreeYearPlan(string PLANNO)
        {
            string sql = @"
                            SELECT 
                                PLANNO, 
                                THREEYEARID, 
                                PLANNAME, 
                                STUDYYEAR, 
                                BUDGET, 
                                SITUATIONTYPE, 
                                SITUATIONDESC
                            FROM 
                                PWSSDTREEYEARPLAN (NOLOCK)
                            WHERE 
                                PLANNO = @PLANNO
                        ";

            var result = await ExecuteQueryAsync<ThreeYearPlanModel>(sql, new { PLANNO }, PWSDBKey);
            return result.ToList();
        }
    }
}
