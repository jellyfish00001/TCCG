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
    public class PlanBasicBDac : Dac, IPlanBasicBDac
    {
        public PlanBasicBDac(
            IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 基本計畫資料存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task SavePlanBasicB(PlanBasicBModel model)
        {
            string sql = $@"
                        INSERT INTO 
                                PWSSDPLANMAIN
                                (
                                PLANYEAR,
                                PLANKIND,
                                OU_ID,
                                PLANNAME,
                                PLANNO,
                                PLANSTARTDATE,
                                PLANENDDATE,
                                AWARDYM,
                                MIDREPORTYM,
                                FINAKREPORTYM,
                                CLOSEYM,
                                CREATEORGOUID,
                                CREATEUNITOUID,
                                CRT_USER,
                                CRT_DATE
                                )
                            VALUES
                                (
                                @PLANYEAR,
                                @PLANKIND,
                                @OU_ID,
                                @PLANNAME,
                                @PLANNO,
                                @PLANSTARTDATE,
                                @PLANENDDATE,
                                @AWARDYM,
                                @MIDREPORTYM,
                                @FINAKREPORTYM,
                                @CLOSEYM,
                                @CREATEORGOUID,
                                @CREATEUNITOUID,
                                @CRT_USER,
                                {DTNow}
                                )
                        ";

                await ExecuteCommandAsync(sql, model, PWSDBKey);
        }

        /// <summary>
        /// 更新基本計畫資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdatePlanBasicB(PlanBasicBModel model)
        {
            string sql = $@"
                            UPDATE PWSSDPLANMAIN
                            SET
                                PLANYEAR = @PLANYEAR,
                                OU_ID = @OU_ID,
                                LABORYN = @LABORYN,
                                PLANNAME = @PLANNAME,
                                BUDGETTYPE = @BUDGETTYPE,
                                PLANSTARTDATE = DATEADD(s, -1, @PLANSTARTDATE),
                                PLANENDDATE = DATEADD(s, -1, @PLANENDDATE),
                                AWARDYM = DATEADD(s, -1, @AWARDYM),
                                MIDREPORTYM = DATEADD(s, -1, @MIDREPORTYM),
                                FINAKREPORTYM = DATEADD(s, -1, @FINAKREPORTYM),
                                CLOSEYM = DATEADD(s, -1, @CLOSEYM),
                                PLANDATETYPE = @PLANDATETYPE,
                                PLANORGINYN = @PLANORGINYN,
                                PUBLICMONEY = @PUBLICMONEY,
                                FUNDMONEY = @FUNDMONEY,
                                FUNDNO = @FUNDNO,
                                APPROVEDYN = @APPROVEDYN,
                                CENTERMONEY = @CENTERMONEY,
                                APPLYAPPROVEDYN = @APPLYAPPROVEDYN,
                                APPROVEDNUMBER = @APPROVEDNUMBER,
                                OTHERMONEY = @OTHERMONEY,
                                OTHERDESC = @OTHERDESC,
                                PLANTOTMONEY = @PLANTOTMONEY,
                                PLANCAUSE = @PLANCAUSE,
                                PLANEXPECTED = @PLANEXPECTED,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE PLANNO = @PLANNO;
                        ";

            await ExecuteCommandAsync(sql, model, PWSDBKey);
        }

        /// <summary>
        /// 取得基本計畫資料
        /// </summary>
        /// <param name="model"></param>
        public async Task<PlanBasicBModel> GetPlanBasicB(string PLANNO)
        {
            string sql = @"
                         SELECT  
                                PLANYEAR,
                                PLANNO,
                                OU_ID,
                                dbo.FN_GetOuName(OU_ID, 3) AS OU_NAME,
                                LABORYN,
                                PLANNAME,
                                PLANSTARTDATE,
                                PLANENDDATE,
                                AWARDYM,
                                MIDREPORTYM,
                                FINAKREPORTYM,
                                CLOSEYM,
                                PLANDATETYPE, 
                                PLANORGINYN, 
                                PUBLICMONEY,
                                FUNDMONEY, 
                                FUNDNO, 
                                APPROVEDYN,
                                CENTERMONEY,
                                APPLYAPPROVEDYN,
                                APPROVEDNUMBER,
                                OTHERMONEY, 
                                OTHERDESC, 
                                PLANTOTMONEY,
                                PLANCAUSE,
                                PLANEXPECTED,
                                IS_SEND_ONTIME  
                           FROM PWSSDPLANMAIN (NOLOCK)
                          WHERE PLANNO = @PLANNO
                            ";

            var result = await ExecuteQueryFirstOrDefaultAsync<PlanBasicBModel>(sql, new { PLANNO }, PWSDBKey);
            return result;
        }

    }
}
