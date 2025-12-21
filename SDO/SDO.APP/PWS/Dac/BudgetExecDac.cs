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
    public class BudgetExecDac : Dac, IBudgetExecDac
    {
        public BudgetExecDac(
            IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }
        /// <summary>
        /// 歷年執行經費新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertBudgetExec(List<BudgetExecModel> models)
        {
            string sql = $@"
                            INSERT INTO PWSSDHISTORYEXE 
                            (
                                PLANNO,
                                EXEYEAR,
                                PUBLICMONEY,
                                GROWRATIO,
                                EXECOUNT,
                                RATIO,
                                EXEDESC,
                                NOBUDGETYN,
                                NOBUDGETDESC,
                                CRT_USER,
                                CRT_DATE
                            ) 
                            VALUES 
                            (
                                @PLANNO,
                                @EXEYEAR,
                                @PUBLICMONEY,
                                @GROWRATIO,
                                @EXECOUNT,
                                @RATIO,
                                @EXEDESC,
                                @NOBUDGETYN,
                                @NOBUDGETDESC,
                                @CRT_USER,
                                {DTNow}
                            )";

            await ExecuteCommandAsync(sql, models, PWSDBKey);
        }

        /// <summary>
        /// 歷年執行經費更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateBudgetExec(List<BudgetExecModel> models)
        {
            string sql = $@"
                            UPDATE PWSSDHISTORYEXE 
                            SET 
                                PUBLICMONEY = @PUBLICMONEY,
                                GROWRATIO = @GROWRATIO,
                                EXECOUNT = @EXECOUNT,
                                RATIO = @RATIO,
                                EXEDESC = @EXEDESC,
                                NOBUDGETYN = @NOBUDGETYN,
                                NOBUDGETDESC = @NOBUDGETDESC,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE 
                                PLANNO = @PLANNO AND 
                                EXEYEAR = @EXEYEAR;
                          ";

            await ExecuteCommandAsync(sql, models, PWSDBKey);
        }

        /// <summary>
        /// 取得歷年執行情形
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        public async Task<List<BudgetExecModel>> GetBudgetExec(string PLANNO)
        {
            string sql = @"
                            SELECT 
                                PLANNO,
                                EXEYEAR,
                                PUBLICMONEY,
                                GROWRATIO,
                                EXECOUNT,
                                RATIO,
                                EXEDESC,
                                NOBUDGETYN,
                                NOBUDGETDESC
                            FROM PWSSDHISTORYEXE (NOLOCK)
                            WHERE PLANNO = @PLANNO
                            ORDER BY EXEYEAR DESC
                          ";

            return (await ExecuteQueryAsync<BudgetExecModel>(sql, new { PLANNO }, PWSDBKey)).ToList();
        }
    }
}
