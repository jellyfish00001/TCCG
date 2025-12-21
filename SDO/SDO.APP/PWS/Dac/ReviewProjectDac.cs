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
using System.Numerics;

namespace SDO.Dac
{
    public class ReviewProjectDac : Dac, IReviewProjectDac
    {
        public ReviewProjectDac(
            IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 取計畫資料
        /// </summary>
        /// <returns></returns>
        public async Task<ReviewProjectModel> GetPlanData(string PLANNO)
        {
            string sql = @"
                             SELECT 
                                    PM.PLANYEAR,
                                    PM.IS_SEND,
                                    PM.PLANKIND,
                                    PM.PLANDATETYPE,
                                    PM.FUNDMONEY,
                                    PM.PLANTOTMONEY,
                                    PM.BUDGETTYPE,
                                    PM.PLANORDERNUMBER,
                                    PV.PLANNO,
                                    PV.PUBLIC1,
                                    PV.PUBLIC2,
                                    PV.FUND1,
                                    PV.FUND2,
                                    PV.ADVIEWDESC,
                                    PV.AUDIT_STATUS,    
                                    PV.SUB_PLANDATETYPE,
                                    PV.PLAN_REF,
                                    PV.EXE_PERFORMANCE
                               FROM PWSSDPLANMAIN PM
                          LEFT JOIN PWSSDVIEW PV ON PM.PLANNO = PV.PLANNO
                              WHERE PM.PLANNO = @PLANNO
                        ";

            var result = await ExecuteQueryFirstOrDefaultAsync<ReviewProjectModel>(sql, new { PLANNO }, PWSDBKey);
            return result;
        }

        /// <summary>
        /// 取計畫資料關聯重大的計畫編號
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetPlanRelevanceNo(string PLANNO)
        {
            string sql = @"
                             SELECT 
                                    IPC_PROJECTNO
                               FROM PWSSDPLANMAIN 
                              WHERE PLANNO = @PLANNO
                        ";

            var result = await ExecuteQueryFirstOrDefaultAsync<string>(sql, new { PLANNO }, PWSDBKey);
            return result;
        }

        /// <summary>
        /// 取計小組審核意見表
        /// </summary>
        /// <returns></returns>
        public async Task<List<AuditTemplateModel>> GetReviewOpinions(string PLANDATETYPE)
        {
            string sql = @"
                             SELECT 
                                    PLANDATETYPE,
                                    SUB_PLANDATETYPE,
                                    PLAN_REF,
                                    EXE_PERFORMANCE,
                                    TEMPLATE
                               FROM PWS_AUDIT_TEMPLATE 
                              WHERE PLANDATETYPE = @PLANDATETYPE
                        ";

            var result = await ExecuteQueryAsync<AuditTemplateModel>(sql, new { PLANDATETYPE }, PWSDBKey);
            return result.ToList();
        }

        /// <summary>
        /// 新建計畫審查紀錄
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task SetPlanReview(ReviewProjectModel model)
        {
            string sql = $@"
                            INSERT INTO PWSSDVIEW
                            (
                            PLANNO,
                            CRT_USER,
                            CRT_DATE
                            )
                            VALUES
                            (
                            @PLANNO,
                            @CRT_USER,
                            {DTNow}
                            )
                          ";
            await ExecuteCommandAsync(sql, model, PWSDBKey);
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task SetReviewProject(ReviewProjectModel model)
        {
            string insertSql = $@"
                                INSERT INTO PWSSDVIEW
                                (
                                PLANNO,
                                PUBLIC1,
                                PUBLIC2,
                                FUND1,
                                FUND2,
                                ADVIEWDESC,
                                CRT_USER,
                                CRT_DATE
                                )
                                VALUES
                                (
                                @PLANNO,
                                @PUBLIC1,
                                @PUBLIC2,
                                @FUND1,
                                @FUND2,
                                @ADVIEWDESC,
                                @ISERVIEW,
                                @CRT_USER,
                                {DTNow}
                                )
                               ";
            await ExecuteCommandAsync(insertSql, model, PWSDBKey);
        }

        /// <summary>
        /// 修改計畫審核
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateReviewProject (ReviewProjectModel model)
        {
            string insertSql = $@"
                                 UPDATE PWSSDVIEW
                                    SET
                                        PUBLIC1 = @PUBLIC1,
                                        PUBLIC2 = @PUBLIC2,
                                        FUND1 = @FUND1,
                                        FUND2 = @FUND2,
                                        SUB_PLANDATETYPE = @SUB_PLANDATETYPE,
                                        PLAN_REF = @PLAN_REF,
                                        EXE_PERFORMANCE = @EXE_PERFORMANCE,
                                        ADVIEWDESC = @ADVIEWDESC,
                                        AUDIT_STATUS = @AUDIT_STATUS,
                                        MDF_USER = @MDF_USER,
                                        MDF_DATE = {DTNow}
                                  WHERE
                                        PLANNO = @PLANNO
                                ";

            await ExecuteCommandAsync(insertSql, model, PWSDBKey);
        }

        /// <summary>
        /// 更新計畫資料關聯重大的計畫編號
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateProjectRelevance(ReviewProjectModel model)
        {
            string sql = $@"
                            UPDATE PWSSDPLANMAIN
                               SET
                                   IPC_PROJECTNO = @IPC_PROJECTNO,
                                   MDF_USER = @MDF_USER,
                                   MDF_DATE = {DTNow}
                             WHERE PLANNO = @PLANNO
                          ";

            await ExecuteCommandAsync(sql, model, PWSDBKey);
        }

        /// <summary>
        /// 取重大關聯資料
        /// </summary>
        /// <returns></returns>
        public async Task<List<OtherProjectModel>> GetOtherProject()
        {
            string sql = @"
                            SELECT 
                                PROJECT_NO,
                                PROJECT_NAME,
                                MASTER_ORGAN_C,
                                MASTER_ORGAN_NAME,
                                EXEC_ORGAN_C,
                                EXEC_ORGAN_NAME,
                                ALL_JOB
                            FROM VW_IPC_PROJECT_TO_TRACKO
                          ";
            var result = await ExecuteQueryAsync<OtherProjectModel>(sql, null);
            return result.ToList();
        }

        /// <summary>
        /// 退回計畫
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        public async Task SendBackProject(ReviewProjectModel model)
        {
            string sql = $@"
                            UPDATE PWSSDPLANMAIN
                            SET
                                IS_SEND = 0,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE
                                PLANNO = @PLANNO
                            ";

            await ExecuteCommandAsync(sql, model, PWSDBKey);
        }

        /// <summary>
        /// 退回小組審核
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task BackProjectReview(ReviewProjectModel model)
        {
            string sql = $@"
                            UPDATE PWSSDVIEW
                            SET
                                AUDIT_STATUS = 0,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE
                                PLANNO = @PLANNO
                            ";

            await ExecuteCommandAsync(sql, model, PWSDBKey);
        }
    }
}
