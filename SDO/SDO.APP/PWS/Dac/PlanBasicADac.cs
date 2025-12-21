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
    public class PlanBasicADac : Dac, IPlanBasicADac
    {
        public PlanBasicADac(
            IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {

        }

        /// <summary>
        /// 取得基本計畫資料
        /// </summary>
        /// <param name="model"></param>
        public async Task<PlanBasicAModel> GetPlanBasicA(string PLANNO)
        {
            string sql = @"
                         SELECT  
                                PLANNAME,
                                OU_ID,
                                dbo.FN_GetOuName(OU_ID, 3) AS OU_NAME,
                                PLANNO,
                                PLANKIND,
                                PLANYEAR,
                                PLANSTARTDATE,
                                PLANENDDATE,
                                CP_KIND,
                                PLANRANGE,
                                OMAINTAINYN, 
                                GENDER_ANALYST_YN,
                                PLANDATETYPE, 
                                ATTA_COST_YN,   
                                PLANORGINYN, 
                                PUBLICMONEY,
                                FUNDMONEY, 
                                FUNDNO, 
                                CENTERMONEY,
                                APPROVEDYN,
                                APPLYAPPROVEDYN,
                                APPROVEDNUMBER,
                                OTHERMONEY, 
                                OTHERDESC, 
                                PLANTOTMONEY,
                                PLANCONTENTENGINE, 
                                PLANCONTENTLAND, 
                                PLANCONTENTINFO, 
                                PLANMAINTAIN, 
                                EXPLAINNECESSITY, 
                                EXPLANBASICINFO, 
                                EXPLANEXECUTE, 
                                EXPLANIMPROVE, 
                                EXPLANFUND,
                                CREATEORGOUID,
                                dbo.FN_GetOuName(CREATEORGOUID, 3) AS ORGOUNAME,
                                CREATEUNITOUID,
								dbo.FN_GetOuName(CREATEUNITOUID, 3) AS UNITOUNAME,
                                RUNWAY_C,
                                IS_SEND_ONTIME
                           FROM PWSSDPLANMAIN (NOLOCK)
                          WHERE PLANNO = @PLANNO
                            ";

            var result = await ExecuteQueryFirstOrDefaultAsync<PlanBasicAModel>(sql, new { PLANNO }, PWSDBKey);
            return result;
        }



        /// <summary>
        /// 複製計畫
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        public async Task<bool> copyProject(PlanBasicAModel model)
        {
            string sql = $@"
                            INSERT INTO PWSSDPLANMAIN
                            (
                                PLANYEAR,
                                PLANNO,
                                OU_ID,
                                CP_KIND,
                                RUNWAY_C,
                                PLANKIND,
                                PLANNAME,
                                FUNDNO,
                                PLANSTARTDATE,
                                PLANENDDATE,
                                AWARDYM,
                                MIDREPORTYM,
                                FINAKREPORTYM,
                                CLOSEYM,
                                LABORYN,
                                PLANDATETYPE,
                                PLANORGINYN,
                                ORGINFILEID,
                                PUBLICMONEY,
                                FUNDMONEY,
                                OTHERMONEY,
                                OTHERDESC,
                                PLANTOTMONEY,
                                CENTERMONEY, 
                                BUDGETTYPE,
                                APPROVEDYN,
                                APPROVEDNUMBER,
                                APPROVEDFIELDID,
                                APPLYAPPROVEDYN,
                                PLANCONTENT,
                                PLANCAUSE,
                                PLANEXPECTED,
                                PLANORDERNUMBER,
                                PLANCONTENTENGINE,
                                PLANCONTENTLAND,
                                PLANCONTENTINFO,
                                EXPLAINNECESSITY,
                                EXPLANBASICINFO,
                                EXPLANEXECUTE,
                                EXPLANIMPROVE,
                                EXPLANFUND,
                                CREATEORGOUID,
                                CREATEUNITOUID,
                                GENDER_ANALYST_YN,
                                ATTA_COST_YN,
                                IPC_PROJECTNO,
                                CRT_USER,
                                CRT_DATE
                            )
                            SELECT
                                PLANYEAR,
                                @PLANNO,
                                OU_ID,
                                CP_KIND,
                                RUNWAY_C,
                                PLANKIND,
                                PLANNAME,
                                FUNDNO,
                                PLANSTARTDATE,
                                PLANENDDATE,
                                AWARDYM,
                                MIDREPORTYM,
                                FINAKREPORTYM,
                                CLOSEYM,
                                LABORYN,
                                PLANDATETYPE,
                                PLANORGINYN,
                                ORGINFILEID,
                                PUBLICMONEY,
                                FUNDMONEY,
                                OTHERMONEY,
                                OTHERDESC,
                                PLANTOTMONEY,
                                CENTERMONEY, 
                                BUDGETTYPE,
                                APPROVEDYN,
                                APPROVEDNUMBER,
                                APPROVEDFIELDID,
                                APPLYAPPROVEDYN,
                                PLANCONTENT,
                                PLANCAUSE,
                                PLANEXPECTED,
                                PLANORDERNUMBER,
                                PLANCONTENTENGINE,
                                PLANCONTENTLAND,
                                PLANCONTENTINFO,
                                EXPLAINNECESSITY,
                                EXPLANBASICINFO,
                                EXPLANEXECUTE,
                                EXPLANIMPROVE,
                                EXPLANFUND,
                                @CREATEORGOUID,
                                @CREATEUNITOUID,
                                GENDER_ANALYST_YN,
                                ATTA_COST_YN,
                                IPC_PROJECTNO,
                                @CRT_USER,
                                {DTNow}
                           FROM PWSSDPLANMAIN (NOLOCK)
                          WHERE PLANNO = @OldPlanNo
                        ";
            var result = await ExecuteCommandAsync(sql, model, PWSDBKey);
            return true;
        }

        /// <summary>
        /// 複製計畫
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        public async Task<bool> copyProjectCheckPoint(PlanBasicAModel model)
        {
            string sql = $@"
                            INSERT INTO PROJECT_CHECKITEM (
                              PROJECT_NO,
                              CHECKITEM_SEQ,
                              CHECKITEM_NAME,
                              PROGRESS,
                              ESTIMATED_STARTDATE,
                              ORI_ESTIMATED_ENDDATE,
                              ESTIMATED_ENDDATE,
                              ACTUAL_ENDDATE,
                              IS_DELAY,
                              CRT_USER,
                              CRT_DATE
                            )
                            SELECT
                              @PLANNO AS PROJECT_NO,
                              CHECKITEM_SEQ,
                              CHECKITEM_NAME,
                              PROGRESS,
                              ESTIMATED_STARTDATE,
                              ORI_ESTIMATED_ENDDATE,
                              ESTIMATED_ENDDATE,
                              ACTUAL_ENDDATE,
                              IS_DELAY,
                              @CRT_USER,
                              {DTNow}
                            FROM PROJECT_CHECKITEM (NOLOCK)
                            WHERE PROJECT_NO = @OldPlanNo
                           ";

            var result = await ExecuteCommandAsync(sql, model, PWSDBKey);
            return true;
        }

        /// <summary>
        /// 取跨年度經費資料
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        public async Task<List<PlanCrossAMTAModel>> GetCrossAMTA(string PLANNO)
        {
            string sql = @"
                         SELECT  
                                IDENTITYFIELD,
                                PLANNO,
                                PLANYEAR,
                                SOURCEKIND,
                                BUDGETTYPE,
                                PLANITEMC,
                                BUDGETCENTRAL,
                                BUDGETLOCAL
                           FROM PROJECT_BUDGET_SOURCE_G (NOLOCK)
                          WHERE PLANNO = @PLANNO
                            ";

            var result = (await ExecuteQueryAsync<PlanCrossAMTAModel>(sql, new { PLANNO }, PWSDBKey)).ToList();
            return result;
        }

        /// <summary>
        /// 從維護年度取最大年度
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetPlanYear()
        {
            string sql = @"
                         SELECT  
                                MAX(PLANYEAR) PLANYEAR
                           FROM PWSSDYEARSET (NOLOCK)
                            ";

            var result = await ExecuteQueryFirstOrDefaultAsync<int>(sql, null, PWSDBKey);
            return result;
        }

        /// <summary>
        /// 基本計畫資料存檔
        /// </summary>
        /// <param name="model"></param>
        public async Task SavePlanBasicA(PlanBasicAModel model)
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
        public async Task UpdatePlanBasicA(PlanBasicAModel model)
        {
            string sql = $@"
                             UPDATE PWSSDPLANMAIN
                                SET
                                    PLANNAME = @PLANNAME,
                                    OU_ID = @OU_ID,
                                    PLANYEAR = @PLANYEAR,
                                    PLANSTARTDATE = DATEADD(s, -1, @PLANSTARTDATE),
                                    PLANENDDATE = DATEADD(s, -1, @PLANENDDATE),
                                    GENDER_ANALYST_YN = @GENDER_ANALYST_YN,
                                    PLANDATETYPE = @PLANDATETYPE,
                                    ATTA_COST_YN = @ATTA_COST_YN,
                                    PLANORGINYN = @PLANORGINYN,
                                    PUBLICMONEY = @PUBLICMONEY,
                                    FUNDMONEY = @FUNDMONEY,
                                    FUNDNO = @FUNDNO,
                                    CENTERMONEY = @CENTERMONEY,
                                    APPROVEDYN = @APPROVEDYN,
                                    APPLYAPPROVEDYN = @APPLYAPPROVEDYN,
                                    APPROVEDNUMBER = @APPROVEDNUMBER,
                                    OTHERMONEY = @OTHERMONEY,
                                    OTHERDESC = @OTHERDESC,
                                    PLANTOTMONEY = @PLANTOTMONEY,
                                    BUDGETTYPE = @BUDGETTYPE,
                                    PLANCONTENTENGINE = @PLANCONTENTENGINE,
                                    PLANCONTENTLAND = @PLANCONTENTLAND,
                                    PLANCONTENTINFO = @PLANCONTENTINFO,
                                    PLANMAINTAIN = @PLANMAINTAIN,
                                    EXPLAINNECESSITY = @EXPLAINNECESSITY,
                                    EXPLANBASICINFO = @EXPLANBASICINFO,
                                    EXPLANEXECUTE = @EXPLANEXECUTE,
                                    EXPLANIMPROVE = @EXPLANIMPROVE,
                                    EXPLANFUND = @EXPLANFUND,
                                    CP_KIND = @CP_KIND,
                                    RUNWAY_C = @RUNWAY_C,
                                    MDF_USER = @MDF_USER,   
                                    MDF_DATE = {DTNow}
                              WHERE PLANNO = @PLANNO;
                            ";

            await ExecuteCommandAsync(sql, model, PWSDBKey);
        }

        /// <summary>
        /// 跨年度經費存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task SaveCrossAMTA(List<PlanCrossAMTAModel> models)
        {
            string sql = $@"
                            INSERT INTO PROJECT_BUDGET_SOURCE_G
                            (
                            PLANNO,
                            PLANYEAR, 
                            SOURCEKIND, 
                            PLANITEMC, 
                            BUDGETCENTRAL, 
                            BUDGETLOCAL,
                            CRT_USER,
                            CRT_DATE
                            )
                            VALUES
                            (
                            @PLANNO,
                            @PLANYEAR, 
                            @SOURCEKIND, 
                            @PLANITEMC, 
                            @BUDGETCENTRAL, 
                            @BUDGETLOCAL,
                            @CRT_USER,
                            {DTNow}
                            );
                         ";

            await ExecuteCommandAsync(sql, models, PWSDBKey);
        }

        /// <summary>
        /// 更新跨年度經費
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateCrossAMTA(List<PlanCrossAMTAModel> models)
        {
            string sql = $@"
                         UPDATE PROJECT_BUDGET_SOURCE_G
                            SET 
                                PLANYEAR = @PLANYEAR,
                                SOURCEKIND = @SOURCEKIND, 
                                BUDGETTYPE = @BUDGETTYPE, 
                                PLANITEMC = @PLANITEMC, 
                                BUDGETCENTRAL = @BUDGETCENTRAL, 
                                BUDGETLOCAL = @BUDGETLOCAL,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                          WHERE 
                                IDENTITYFIELD = @IDENTITYFIELD;
                        ";

            await ExecuteCommandAsync(sql, models, PWSDBKey);
        }

        /// <summary>
        /// 跨年度經費存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task DeleteCrossAMTA(List<PlanCrossAMTAModel> models)
        {
            string sql = @"
                           DELETE FROM PROJECT_BUDGET_SOURCE_G
                                 WHERE IDENTITYFIELD = @IDENTITYFIELD;
                            ";

            await ExecuteCommandAsync(sql, models, PWSDBKey);
        }

        /// <summary>
        /// 取檢核點資料
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProjectCusCheckpointModel>> GetProjectCheckPoint(string PROJECT_NO)
        {
            string sql = @"
                         SELECT  
                                SEQ,
                                PROJECT_NO,
                                CHECKITEM_SEQ,
                                CHECKITEM_NAME,
                                PROGRESS,
                                ESTIMATED_ENDDATE
                           FROM PROJECT_CHECKITEM (NOLOCK)
                          WHERE PROJECT_NO = @PROJECT_NO
                          ORDER BY PROGRESS
                            ";

            var result = (await ExecuteQueryAsync<ProjectCusCheckpointModel>(sql, new { PROJECT_NO }, PWSDBKey)).ToList();
            return result;
        }   

        /// <summary>
        /// 檢核點刪除(多筆)
        /// </summary>
        /// <returns></returns>
        public async Task DeleteProjectCheckPoints(string PROJECT_NO)
        {
            string sql = @"
                           DELETE FROM PROJECT_CHECKITEM
                                 WHERE PROJECT_NO = @PROJECT_NO;
                          ";

            await ExecuteCommandAsync(sql, new { PROJECT_NO }, PWSDBKey);
        }

        /// <summary>
        /// 檢核點新增
        /// </summary>
        /// <returns></returns>
        public async Task InsertProjectCheckPoint(List<ProjectCusCheckpointModel> models)
        {
            string sql = $@"
                            INSERT INTO 
                                PROJECT_CHECKITEM
                                (
                                PROJECT_NO,
                                CHECKITEM_SEQ,
                                CHECKITEM_NAME,
                                PROGRESS,
                                ESTIMATED_ENDDATE,
                                CRT_USER,
                                CRT_DATE
                                )
                                VALUES
                                (
                                @PROJECT_NO,
                                @CHECKITEM_SEQ,
                                @CHECKITEM_NAME,
                                @PROGRESS,
                                @ESTIMATED_ENDDATE,
                                @CRT_USER,
                                {DTNow}
                                );
                          ";

            await ExecuteCommandAsync(sql, models, PWSDBKey);
        }
        /// <summary>
        /// 檢核點更改
        /// </summary>
        /// <returns></returns>
        public async Task UpdateProjectCheckPoint(List<ProjectCusCheckpointModel> models)
        {
            string sql = $@"
                         UPDATE PROJECT_CHECKITEM
                            SET
                                PROJECT_NO = @PROJECT_NO,
                                CHECKITEM_SEQ = @CHECKITEM_SEQ,
                                CHECKITEM_NAME = @CHECKITEM_NAME,
                                PROGRESS = @PROGRESS,
                                ESTIMATED_ENDDATE = @ESTIMATED_ENDDATE,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                          WHERE SEQ = @SEQ;
                          ";

            await ExecuteCommandAsync(sql, models, PWSDBKey);
        }

        /// <summary>
        /// 檢核點刪除
        /// </summary>
        /// <returns></returns>
        public async Task DeleteProjectCheckPoint(List<ProjectCusCheckpointModel> models)
        {
            string sql = @"
                            DELETE FROM 
                                        PROJECT_CHECKITEM
                                  WHERE SEQ = @SEQ;
                          ";

            await ExecuteCommandAsync(sql, models, PWSDBKey);
        }

        /// <summary>
        /// 取得計畫編號流水號
        /// </summary>
        /// <param name="projectNoStart6Char">計畫編號前6碼</param>
        /// <returns></returns>
        public async Task<string> GetProjectNoSeq(string projectNoStart6Char)
        {
            string sql = @"SELECT ISNULL(MAX(CAST(RIGHT(PLANNO,3) AS INT)),0)+1 AS NUM 
                           FROM PWSSDPLANMAIN (NOLOCK)
                           WHERE PLANNO LIKE  @projectNoStart6Char + '%'";
            return await ExecuteQueryFirstOrDefaultAsync<string>(sql, new { projectNoStart6Char }, PWSDBKey);
        }
    }
}
