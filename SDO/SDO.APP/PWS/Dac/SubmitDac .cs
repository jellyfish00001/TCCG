using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
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
    public class SubmitDac : Dac, ISubmitDac
    {
        public SubmitDac(
            IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 檢查當期執行情形是否已送出
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<bool> CheckProjectFillIsSend(string PLANNO)
        {
            string sql = @"SELECT 
                                  IS_SEND
                             FROM PWSSDPLANMAIN
                            WHERE PLANNO = @PLANNO
                          ";
            return await ExecuteQueryFirstOrDefaultAsync<int>(sql, new { PLANNO}, PWSDBKey) > 0;

        }

        /// <summary>
        /// 計畫送出
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        public async Task ProjectFillSubmit(string PLANNO)
        {
            string sql = @"UPDATE PWSSDPLANMAIN
                              SET IS_SEND = 1,
                                  IS_SEND_ONTIME = 1
                            WHERE PLANNO = @PLANNO
                          ";
            await ExecuteCommandAsync(sql, new { PLANNO }, PWSDBKey);
        }

        /// <summary>
        /// 檢查計畫經費是否一致
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        public async Task<bool> CheckProjectMoney(string PLANNO)
        {
            string sql = @"
                            SELECT 
	                            CASE 
                                    WHEN MAX(ISNULL(PUBLICMONEY, 0) + ISNULL(FUNDMONEY, 0) + ISNULL(OTHERMONEY, 0) + ISNULL(CENTERMONEY, 0)) = SUM (fund.FUNDTOT) THEN 1
                                    ELSE 0
                                END AS MoneyCheck
                            FROM 
                               PWSSDPLANMAIN AS main
                            LEFT JOIN 
                               PWSSDPLANFUND AS fund ON main.PLANNO = fund.PLANNO
                            WHERE 
                                main.PLANNO = @PLANNO
                            GROUP BY 
                                main.PLANNO;
                          ";
            return await ExecuteQueryFirstOrDefaultAsync<int>(sql, new { PLANNO }, PWSDBKey) > 0;
        }

        /// <summary>
        /// 檢查檢核點日期是否重複
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <param name="FUNDTOT"></param>
        /// <returns></returns>
        public async Task<List<string>> ChkPlanBasicAValid(string PLANNO)
        {
            string sql = @"
                            SELECT
                                CASE
                                    WHEN ESTIMATED_ENDDATE >= LAG(ESTIMATED_ENDDATE) OVER (ORDER BY PROGRESS) THEN 'Yes'
                                    WHEN LAG(ESTIMATED_ENDDATE) OVER (ORDER BY PROGRESS) IS NULL AND ESTIMATED_ENDDATE IS NOT NULL THEN 'Yes'
                                    ELSE 'No'
                                END AS IS_INCREASING
                             FROM TYCG_PWS_M.dbo.PROJECT_CHECKITEM
                            WHERE PROJECT_NO = @PLANNO
                            ORDER BY PROGRESS;
                          ";
            var result = await ExecuteQueryAsync<string>(sql, new { PLANNO }, PWSDBKey);
            return result.ToList();
        }

    }
}
