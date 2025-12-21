using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class ProjectDac : Dac, IProjectDac
    {
        private readonly IUserData user;
        public ProjectDac(IConnectionControlCenter connectionControlCenter,
           IHttpContextAccessor httpContextAccessor,
           ISqlTrace trace,
           IUserProfile profile,
           IParameterAdaptor ParameterAdaptor,
           IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
            user = profile.GetLoginUser();
        }

        #region 計畫章節
        /// <summary>
        /// 取得計畫章節表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ProjectChapterListModel>> GetProjectChapter(ProjectChapterQueryModel model)
        {
            StringBuilder sql = new 
            (@"
                SELECT  CHAPTER AS title,
                        CHAPTER_ID,
                        STAGE,
                        SOURCE_PATH,
                        SORT_ORDER,
                        SYMBOL
                FROM PROJECT_CHAPTER (NOLOCK)
                WHERE 1=1 ");

            // 依照每個子系統使用者角色，決定要顯示哪些章節
            switch (model.ApId)
            {
                case "IPC3":
                case "INN":
                case "PWS":
                case "RD2":
                    sql.AppendLine("AND USER_ROLE IN (@USER_ROLE,'1')");
                    break;
            }

            // 計畫作業
            if (!string.IsNullOrEmpty(model.OPERATION))
            {
                sql.AppendLine("AND OPERATION IN (@OPERATION,'S1')");
            }

            sql.AppendLine("ORDER BY SORT_ORDER");

            return (await ExecuteQueryAsync<ProjectChapterListModel>(sql.ToString(), model, GetAPDBKey(model.ApId))).ToList();
        }

        #endregion

        #region 取得計劃基本資料
        /// <summary>
        /// 取得計畫基本資料 或 計畫基本資料歷程檔
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="logId">取歷程檔</param>
        /// <returns></returns>
        public async Task<ProjectBasicModel> GetProjectBasic(string projectNo, int logId)
        {
            string table = logId > 0 ? "PROJECT_BASIC_HIS" : "PROJECT_BASIC";
            string logIdSql = logId > 0 ? "AND LOG_ID = @LOG_ID" : string.Empty;

            string sql = $@"
                SELECT  
	                M1.PROJECT_NO,
                    M1.PROJECT_NAME,
                    M1.PROJECT_YEAR,
                    M1.PROJECT_TYPE,
                    M1.MASTER_ORGAN_C,
                    M1.MASTER_UNDERTAKER_C,
	                D2.USR_NAME AS MASTER_UNDERTAKER_NAME,
                    M1.EXEC_ORGAN_C,
                    M1.EXEC_UNDERTAKER_C,
	                D3.USR_NAME AS EXEC_UNDERTAKER_NAME,
                    M1.BUDGET_HOLD_ORGAN_C,
                    M1.BUDGET_HOLD_UNDERTAKER_C,
	                D4.USR_NAME AS BUDGET_HOLD_UNDERTAKER_NAME,
                    M1.REVIEWITEM,
                    M1.TOWN_C,
                    M1.TOWN_M,
                    M1.PROJECT_LOCATION,
                    M1.X_COORD,
                    M1.Y_COORD,
                    M1.ALL_JOB,
                    M1.PROJECT_BENEFIT,
                    M1.MEMO,
                    M1.CREATEDTIME,
                    M1.PROJECT_STATUS,
                    M1.MEMO_EVALUATION,
                    STUFF((
                        SELECT '、' + MM2.SET_VALUE 
                        FROM PROJECT_MAPPING_DATA (NOLOCK) MM1
                        INNER JOIN SET_PARAM (NOLOCK) MM2 
			                ON MM1.SET_TYPE = MM2.SET_TYPE AND MM2.SET_ITEM = 'SPEC_NOTE'
                        WHERE PROJECT_NO = @PROJECT_NO
                        FOR XML PATH('')), 1, 1, '') AS SPEC_NOTE,
                    D1.REVIEW_COMMENTS,
                    M1.SCORE_A,
                    dbo.FN_GET_IPC_PARAM(M1.PROJECT_STATUS,'B1') as TUBE_STATUS_DESC
                FROM {table} (NOLOCK) M1
                LEFT JOIN (
                    SELECT TOP 1 PROJECT_NO, REVIEW_COMMENTS
                    FROM PROJECT_AUDIT (NOLOCK)
                    WHERE PROJECT_NO = @PROJECT_NO
		                AND PLAN_REVIEW_TYPE = 'P1'
                    ORDER BY LOG_ID DESC
                ) D1 ON D1.PROJECT_NO = M1.PROJECT_NO
                LEFT JOIN {SC30_M}.SCUSERM D2 on D2.USR_ID = M1.MASTER_UNDERTAKER_C
                LEFT JOIN {SC30_M}.SCUSERM D3 on D3.USR_ID = M1.EXEC_UNDERTAKER_C
                LEFT JOIN {SC30_M}.SCUSERM D4 on D4.USR_ID = BUDGET_HOLD_UNDERTAKER_C
                WHERE M1.PROJECT_NO = @PROJECT_NO 
                    {logIdSql}";

            return await ExecuteQueryFirstOrDefaultAsync<ProjectBasicModel>(sql, new
            {
                PROJECT_NO = projectNo,
                LOG_ID = logId
            });
        }

        /// <summary>
        /// 取得計畫經費來源
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="logId">取歷程檔</param>
        /// <returns></returns>
        public async Task<List<ProjectBudgetSourceGModel>> GetProjectBudgetSourceG(string projectNo, int logId)
        {
            string table = logId > 0 ? "PROJECT_BUDGET_SOURCE_G_HIS" : "PROJECT_BUDGET_SOURCE_G";
            string sql = $@"
                SELECT IDENTITY_FIELD,
                        PROJECT_NO,
                        PLAN_YEAR,
                        BUDGET_CLASS,
                        PLAN_ITEM_C,
                        BUDGET_CENTRAL,
                        PLAN_ITEM_L,
                        BUDGET_LOCAL
                FROM {table}　(NOLOCK)
                WHERE PROJECT_NO = @PROJECT_NO";
            if (logId > 0)
            {
                sql += " AND LOG_ID = @LOG_ID";
            }

            sql += " ORDER BY PLAN_YEAR";
            return (await ExecuteQueryAsync<ProjectBudgetSourceGModel>(sql, new { PROJECT_NO = projectNo, LOG_ID = logId })).ToList();
        }

        /// <summary>
        /// 取得計畫建設類別
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="logId">取歷程檔</param>
        /// <returns></returns>
        public async Task<List<ProjectBuildKindModel>> GetProjectBuildKind(string projectNo, int logId)
        {
            string table = logId > 0 ? "PROJECT_BUILD_KIND_HIS" : "PROJECT_BUILD_KIND";
            string sql = $@"
                SELECT BUILD_ID,
                        PROJECT_NO,
                        BUILD_KIND_TYPE,
                        BUILD_KIND
                FROM {table} (NOLOCK)
                WHERE PROJECT_NO = @PROJECT_NO";
            if (logId > 0)
            {
                sql += " AND LOG_ID = @LOG_ID";
            }
            return (await ExecuteQueryAsync<ProjectBuildKindModel>(sql, new { PROJECT_NO = projectNo, LOG_ID = logId })).ToList();
        }

        /// <summary>
        /// 取得計畫協辦機關
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="logId">取歷程檔</param>
        /// <returns></returns>
        public async Task<List<ProjectAsstOrgModel>> GetProjectAsstOrg(string projectNo, int logId)
        {
            string table = logId > 0 ? "PROJECT_ASST_ORG_HIS" : "PROJECT_ASST_ORG";
            
            string sql = $@"
                SELECT ASST_ID,
                       PROJECT_NO,
                       ASSISTANT_ORGAN_C,
                       ASSISTANT_UNDERTAKER_C,
		               S.USR_NAME AS ASSISTANT_UNDERTAKER_C_NAME
                FROM {table} P
                LEFT JOIN {SC30_M}.SCUSERM S on S.USR_ID = P.ASSISTANT_UNDERTAKER_C
                WHERE PROJECT_NO = @PROJECT_NO";
            if (logId > 0)
            {
                sql += " AND LOG_ID = @LOG_ID";
            }
            return (await ExecuteQueryAsync<ProjectAsstOrgModel>(sql, new { PROJECT_NO = projectNo, LOG_ID = logId })).ToList();
        }
        #endregion

        #region AddMdf 計畫基本資料、計畫經費來源、計畫建設類別、計畫協辦機關
        #region 計畫基本資料
        /// <summary>
        /// 新增計畫基本資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns>回傳新增後，產生的列管編號</returns>
        public void InsertProjectBasic(ProjectBasicModel model)
        {
            string sql = $@"
                INSERT INTO PROJECT_BASIC 
                    (PROJECT_NO
                    ,PROJECT_NAME
                    ,PROJECT_YEAR
                    ,PROJECT_TYPE
                    ,MASTER_ORGAN_C
                    ,MASTER_UNDERTAKER_C
                    ,EXEC_ORGAN_C
                    ,EXEC_UNDERTAKER_C
                    ,BUDGET_HOLD_ORGAN_C
                    ,BUDGET_HOLD_UNDERTAKER_C
                    ,REVIEWITEM
                    ,TOWN_C
                    ,TOWN_M
                    ,PROJECT_LOCATION
                    ,X_COORD
                    ,Y_COORD
                    ,ALL_JOB
                    ,PROJECT_BENEFIT
                    ,MEMO
                    ,CREATEDTIME
                    ,PIS_SELECT
                    ,CRT_USER
                    ,CRT_DATE
                    ,MDF_USER
                    ,MDF_DATE )
                VALUES
                    (@PROJECT_NO
                    ,@PROJECT_NAME
                    ,@PROJECT_YEAR
                    ,@PROJECT_TYPE
                    ,@MASTER_ORGAN_C
                    ,@MASTER_UNDERTAKER_C
                    ,@EXEC_ORGAN_C
                    ,@EXEC_UNDERTAKER_C
                    ,@BUDGET_HOLD_ORGAN_C
                    ,@BUDGET_HOLD_UNDERTAKER_C
                    ,@REVIEWITEM
                    ,@TOWN_C
                    ,@TOWN_M
                    ,@PROJECT_LOCATION
                    ,@X_COORD
                    ,@Y_COORD
                    ,@ALL_JOB
                    ,@PROJECT_BENEFIT
                    ,@MEMO
                    ,GETDATE()
                    ,0
                    ,@CRT_USER
                    ,GETDATE()
                    ,@MDF_USER
                    ,GETDATE() )";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 修改計畫基本資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns>回傳列管編號</returns>
        public void UpdateProjectBasic(ProjectBasicModel model)
        {
            string sql = $@"
                UPDATE PROJECT_BASIC
                SET PROJECT_NAME = @PROJECT_NAME,
                    PROJECT_YEAR = @PROJECT_YEAR,
                    PROJECT_TYPE = @PROJECT_TYPE,
                    MASTER_ORGAN_C = @MASTER_ORGAN_C,
                    MASTER_UNDERTAKER_C = @MASTER_UNDERTAKER_C,
                    EXEC_ORGAN_C = @EXEC_ORGAN_C,
                    EXEC_UNDERTAKER_C = @EXEC_UNDERTAKER_C,
                    BUDGET_HOLD_ORGAN_C = @BUDGET_HOLD_ORGAN_C,
                    BUDGET_HOLD_UNDERTAKER_C = @BUDGET_HOLD_UNDERTAKER_C,
                    REVIEWITEM = @REVIEWITEM,
                    TOWN_C = @TOWN_C,
                    TOWN_M = @TOWN_M,
                    PROJECT_LOCATION = @PROJECT_LOCATION,
                    X_COORD = @X_COORD,
                    Y_COORD = @Y_COORD,
                    ALL_JOB = @ALL_JOB,
                    PROJECT_BENEFIT = @PROJECT_BENEFIT,
                    MEMO = @MEMO,
                    MDF_USER = @MDF_USER,
                    MDF_DATE = {DTNow}
                WHERE PROJECT_NO = @PROJECT_NO ";
            ExecuteCommand(sql, model);
        }

        #endregion

        #region 計畫經費來源
        /// <summary>
        /// 新增計畫經費來源
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int InsertProjectBudgetSourceG(ProjectBudgetSourceGModel model)
        {
            string sql = $@"
                INSERT INTO PROJECT_BUDGET_SOURCE_G
                    (PROJECT_NO
                    ,PLAN_YEAR
                    ,BUDGET_CLASS
                    ,PLAN_ITEM_C
                    ,BUDGET_CENTRAL
                    ,PLAN_ITEM_L
                    ,BUDGET_LOCAL
                    ,CRT_USER
                    ,CRT_DATE
                    ,MDF_USER
                    ,MDF_DATE)
                OUTPUT INSERTED.IDENTITY_FIELD
                VALUES
                    (@PROJECT_NO
                    ,@PLAN_YEAR
                    ,@BUDGET_CLASS
                    ,@PLAN_ITEM_C
                    ,@BUDGET_CENTRAL
                    ,@PLAN_ITEM_L
                    ,@BUDGET_LOCAL
                    ,@CRT_USER
                    ,{DTNow}
                    ,@MDF_USER
                    ,{DTNow})";
            return ExecuteQuery<int>(sql, model, isSetUSER: true).FirstOrDefault();
        }

        /// <summary>
        /// 修改計畫經費來源
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectBudgetSourceG(ProjectBudgetSourceGModel model)
        {
            string sql = $@"
                UPDATE PROJECT_BUDGET_SOURCE_G
                SET PLAN_YEAR = @PLAN_YEAR,
                    BUDGET_CLASS = @BUDGET_CLASS,
                    PLAN_ITEM_C = @PLAN_ITEM_C,
                    BUDGET_CENTRAL = @BUDGET_CENTRAL,
                    PLAN_ITEM_L = @PLAN_ITEM_L,
                    BUDGET_LOCAL = @BUDGET_LOCAL,
                    MDF_USER = @MDF_USER,
                    MDF_DATE = {DTNow}
                WHERE IDENTITY_FIELD = @IDENTITY_FIELD";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 刪除計計畫經費來源
        /// </summary>
        /// <param name="identityField"></param>
        public void DeleteProjectBudgetSourceG(int identityField)
        {
            string sql = @"
                DELETE FROM PROJECT_BUDGET_SOURCE_G
                WHERE IDENTITY_FIELD = @IDENTITY_FIELD";
            ExecuteCommand(sql, new { IDENTITY_FIELD = identityField });
        }
        #endregion

        #region 計畫建設類別
        /// <summary>
        /// 新增計畫建設類別
        /// </summary>
        /// <param name="model"></param>
        /// <param name="projectNo"></param>
        public void InsertProjectBuildKind(List<ProjectBuildKindModel> model, string projectNo)
        {
            string sql = $@"
                INSERT INTO PROJECT_BUILD_KIND
                    (PROJECT_NO
                    ,BUILD_KIND_TYPE
                    ,BUILD_KIND
                    ,CRT_USER
                    ,CRT_DATE
                    ,MDF_USER
                    ,MDF_DATE)
                VALUES
                    (@PROJECT_NO
                    ,@BUILD_KIND_TYPE
                    ,@BUILD_KIND
                    ,@USER
                    ,{DTNow}
                    ,@USER
                    ,{DTNow})";

            var param = model.Select(x =>
            {
                return new
                {
                    PROJECT_NO = projectNo,
                    BUILD_KIND_TYPE = x.BUILD_KIND_TYPE,
                    BUILD_KIND = x.BUILD_KIND,
                    USER = UserId
                };
            }).ToList();

            ExecuteCommand(sql, param);
        }


        /// <summary>
        /// 刪除計畫建設類別
        /// </summary>
        /// <param name="buildId"></param>
        public void DeleteProjectBuildKind(string projectNo)
        {
            string sql = @"
                DELETE PROJECT_BUILD_KIND
                WHERE PROJECT_NO = @PROJECT_NO";
            ExecuteCommand(sql, new { PROJECT_NO = projectNo });
        }
        #endregion

        #region 計畫協辦機關
        /// <summary>
        /// 新增計畫協辦機關
        /// </summary>
        /// <param name="model"></param>
        public void InsertProjectAsstOrg(ProjectAsstOrgModel model)
        {
            string sql = $@"
                INSERT INTO PROJECT_ASST_ORG
                    (PROJECT_NO
                    ,ASSISTANT_ORGAN_C
                    ,ASSISTANT_UNDERTAKER_C
                    ,CRT_USER
                    ,CRT_DATE
                    ,MDF_USER
                    ,MDF_DATE)
                VALUES
                    (@PROJECT_NO
                    ,@ASSISTANT_ORGAN_C
                    ,@ASSISTANT_UNDERTAKER_C
                    ,@CRT_USER
                    ,{DTNow}
                    ,@MDF_USER
                    ,{DTNow})";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 修改計畫協辦機關
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectAsstOrg(ProjectAsstOrgModel model)
        {
            string sql = $@"
                UPDATE PROJECT_ASST_ORG
                SET ASSISTANT_ORGAN_C = @ASSISTANT_ORGAN_C,
                    ASSISTANT_UNDERTAKER_C = @ASSISTANT_UNDERTAKER_C,
                    MDF_USER = @MDF_USER,
                    MDF_DATE = {DTNow}
                WHERE ASST_ID = @ASST_ID";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 刪除計畫協辦機關
        /// </summary>
        /// <param name="asstId"></param>
        public void DeleteProjectAsstOrg(int asstId)
        {
            string sql = @"
                DELETE FROM PROJECT_ASST_ORG
                WHERE ASST_ID = @ASST_ID";
            ExecuteCommand(sql, new { ASST_ID = asstId });
        }
        #endregion
        #endregion

        #region 計劃檢核點設定
        /// <summary>
        /// 取得計劃檢核點設定資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="LOG_ID">取歷程檔</param>
        /// <returns></returns>
        public async Task<ProjectCheckpointModel> GetProjectCheckpoint(string PROJECT_NO, int LOG_ID)
        {
            string table1 = LOG_ID > 0 ? "PROJECT_BASIC_HIS" : "PROJECT_BASIC";
            string table2 = LOG_ID > 0 ? "PROJECT_CONTROL_EXECUTE_HIS" : "PROJECT_CONTROL_EXECUTE";

            string sql = $@"SELECT   main.PROJECT_NO,
                                    main.PROJECT_NAME,
                                    main.CP_KIND,
                                    main.RUNWAY_C,
                                    main.RUNWAY_C as OLD_RUNWAY_C,
                                    main.LAST_DATE_HISTORY,
                                    main.LAST_DATE_HISTORY_REASON,
                                    main.EACH_LAST_MONTH_HISTORY,
                                    main.EACH_LAST_MONTH_HISTORY_REASON,
                                    main.MEMO_CHK_POINT,
                                    exe.CONTROL_DATE1
                            FROM {table1} main (NOLOCK)
                            LEFT JOIN {table2} exe (NOLOCK)
                                on main.PROJECT_NO = exe.PROJECT_NO
                            WHERE main.PROJECT_NO = @PROJECT_NO";
            return await ExecuteQueryFirstOrDefaultAsync<ProjectCheckpointModel>(sql, new { PROJECT_NO, LOG_ID });
        }

        /// <summary>
        /// 取得計劃自訂檢核點資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="LOG_ID">取歷程檔</param>
        /// <returns></returns>
        public async Task<List<ProjectCusCheckpointModel>> GetProjectCusCheckpoint(string PROJECT_NO, int LOG_ID)
        {
            string table = LOG_ID > 0 ? "PROJECT_CHECKITEM_HIS" : "PROJECT_CHECKITEM";
            string condtion = LOG_ID > 0 ? "  AND LOG_ID = @LOG_ID" : "";
            string sql = $@"SELECT m.SEQ,
                                  m.PROJECT_NO,
                                  m.CHECKITEM_SEQ,
                                  m.CHECKITEM_NAME,
                                  m.PROGRESS,
                                  m.ESTIMATED_STARTDATE,
                                  m.ESTIMATED_ENDDATE,
                                  m.ACTUAL_ENDDATE,
                                  m.PCC_ESTIMATED_ENDDATE,
                                  m.PCC_ACTUAL_ENDDATE,
                                  params.CTRL_POINT,
                                  m.MDF_DATE
                           FROM {table} (NOLOCK) m
                           LEFT JOIN CODE_CHECKPOINT_ITEM (NOLOCK) params 
                               ON m.CHECKITEM_SEQ = params.SEQ 
                           WHERE PROJECT_NO = @PROJECT_NO
                           {condtion}
                           ORDER BY PROGRESS, ESTIMATED_ENDDATE";

            return (await ExecuteQueryAsync<ProjectCusCheckpointModel>(sql, new { PROJECT_NO, LOG_ID })).ToList();
        }

        /// <summary>
        /// 更新檢核點設定 - 計畫基本資料
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectCheckpoint(ProjectCheckpointModel model)
        {
            string sql = $@"update PROJECT_BASIC 
                           set CP_KIND = @CP_KIND,
                               RUNWAY_C = @RUNWAY_C,
                               PROJECT_LAST_DATE = @PROJECT_LAST_DATE,
                               MEMO_CHK_POINT = @MEMO_CHK_POINT,
                               MDF_USER = @MDF_USER,
                               MDF_DATE = {DTNow}
                           where PROJECT_NO = @PROJECT_NO";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 更新計畫預定實際期程
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectControlExecute(ProjectCheckpointModel model)
        {
            string sql = $@"update PROJECT_CONTROL_EXECUTE 
                            set CONTROL_DATE1 = @CONTROL_DATE1,
                               CONTROL_DATE6 = @CONTROL_DATE6,
                               MDF_USER = @MDF_USER,
                               MDF_DATE = {DTNow}
                            where PROJECT_NO = @PROJECT_NO
                            -- 若沒資料則新增
                            IF @@ROWCOUNT = 0
                            BEGIN
                                INSERT INTO PROJECT_CONTROL_EXECUTE 
                                     (PROJECT_NO,
                                      CONTROL_DATE1,
                                      CONTROL_DATE6,
                                      CRT_USER,
                                      CRT_DATE,
                                      MDF_USER,
                                      MDF_DATE)
                                VALUES 
                                     (@iPROJECT_NO,
                                      @iCONTROL_DATE1,
                                      @iCONTROL_DATE6,
                                      @iCRT_USER,
                                      {DTNow},
                                      @iMDF_USER,
                                      {DTNow});
                            END";
            ExecuteCommand(sql, new
            {
                model.PROJECT_NO,
                model.CONTROL_DATE1,
                model.CONTROL_DATE6,
                MDF_USER = UserId,
                iPROJECT_NO = model.PROJECT_NO,
                iCONTROL_DATE1 = model.CONTROL_DATE1,
                iCONTROL_DATE6 = model.CONTROL_DATE6,
                iCRT_USER = UserId,
                iMDF_USER = UserId
            });
        }

        /// <summary>
        /// 新增自訂檢核點設定資料
        /// </summary>
        /// <param name="models"></param>
        public void InsertProjectCustomChkItemDate(List<ProjectCusCheckpointModel> models)
        {
            string sql = $@"INSERT INTO PROJECT_CHECKITEM (
                                PROJECT_NO,
                                CHECKITEM_SEQ,
                                CHECKITEM_NAME,
                                PROGRESS,
                                ESTIMATED_STARTDATE,
                                ESTIMATED_ENDDATE,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            VALUES(
                                @PROJECT_NO,
                                @CHECKITEM_SEQ,
                                @CHECKITEM_NAME,
                                @PROGRESS,
                                @ESTIMATED_STARTDATE,
                                @ESTIMATED_ENDDATE,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            ExecuteCommand(sql, models);
        }

        /// <summary>
        /// 更新自訂檢核點設定資料
        /// </summary>
        /// <param name="models"></param>
        public void UpdateProjectCustomChkItemDate(List<ProjectCusCheckpointModel> models)
        {
            string sql = $@"update PROJECT_CHECKITEM
                            set
                                PROGRESS = @PROGRESS,
                                ESTIMATED_STARTDATE = @ESTIMATED_STARTDATE,
	                            ESTIMATED_ENDDATE = @ESTIMATED_ENDDATE,
	                            MDF_USER = @MDF_USER,
	                            MDF_DATE = {DTNow}
                            where SEQ = @SEQ";
            ExecuteCommand(sql, models);
        }

        /// <summary>
        /// 刪除自訂檢核點設定資料
        /// </summary>
        /// <param name="models"></param>
        public void DeleteProjectCustomChkItemDate(List<ProjectCusCheckpointModel> models)
        {
            string sql = "delete PROJECT_CHECKITEM where SEQ = @SEQ";
            ExecuteCommand(sql, models);
        }

        /// <summary>
        /// 刪除自訂檢核點設定資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        public void DeleteProjectCustomChkItemDate(string PROJECT_NO)
        {
            string sql = @"delete PROJECT_CHECKITEM where PROJECT_NO = @PROJECT_NO";
            ExecuteCommand(sql, new { PROJECT_NO });
        }

        /// <summary>
        /// 判斷計畫最後一個檢核點是否有填實際完成日期
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<bool> IsLasttActualEnddate(string PROJECT_NO)
        {
            string sql = $@"SELECT TOP 1 CAST((CASE WHEN ACTUAL_ENDDATE IS NULL THEN 0 ELSE 1 END) AS BIT)
                            FROM PROJECT_CHECKITEM (NOLOCK) 
                            WHERE PROJECT_NO = @PROJECT_NO
                            AND PROGRESS = 100 --最後一筆通常都是100%
                            ORDER BY PROGRESS DESC";
            return (await ExecuteQueryAsync<bool>(sql, new { PROJECT_NO })).FirstOrDefault();
        }
        #endregion

        #region 計畫送審
        /// <summary>
        /// 是否在填報週期內
        /// </summary>
        /// <returns></returns>
        public bool IsInTheFillCycle()
        {
            string sql = @"select count(SEQ)
                            from PROJECT_FILL_CYCLE (nolock)
                            where GETDATE() between FILL_START_DATE and FILL_END_DATE";
            return ExecuteQueryFirstOrDefault<int>(sql) > 0;
        }

        /// <summary>
        /// 更新計劃狀態 & 管考備註
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="PROJECT_STATUS"></param>
        public void UpdateProjectStatusAndMemo(string PROJECT_NO, string PROJECT_STATUS, string MEMO_EVALUATION)
        {
            string sql = $@"UPDATE PROJECT_BASIC 
                            SET PROJECT_STATUS = @PROJECT_STATUS, ";

            if (!string.IsNullOrEmpty(MEMO_EVALUATION))
            {
                sql += "MEMO_EVALUATION = @MEMO_EVALUATION, ";
            }
            sql += $@"MDF_USER = @MDF_USER,
                      MDF_DATE = { DTNow}
                      WHERE PROJECT_NO = @PROJECT_NO";
            ExecuteCommand(sql, new
            {
                PROJECT_NO,
                PROJECT_STATUS,
                MEMO_EVALUATION,
                MDF_USER = UserId
            });
        }

        /// <summary>
        /// 新增計畫工程進度 取PROJECT_FILL_CYCLE最新年月
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        public void InsertProjectEngineeringProgress(string PROJECT_NO)
        {
            string sql = $@"insert into PROJECT_ENGINEERING_PROGRESS 
                                (PROJECT_NO, YEAR, MONTH, CRT_USER, CRT_DATE, MDF_USER, MDF_DATE)
                            select top 1  
	                            @PROJECT_NO, 
	                            PROJECT_YEAR as YEAR,
	                            PROJECT_MONTH as MONTH,
	                            @UserId as CRT_USER,
	                            {DTNow} as CRT_DATE,
	                            @UserId as MDF_USER,
	                            {DTNow} as MDF_DATE
                            from PROJECT_FILL_CYCLE
                            order by PROJECT_YEAR desc, PROJECT_MONTH desc";
            ExecuteCommand(sql, new { PROJECT_NO, UserId });
        }

        /// <summary>
        /// 新增計畫基本資料異動紀錄檔
        /// </summary>
        /// <param name="model"></param>
        public int InsertProjectBasicLog(ProjectBasicLogModel model)
        {
            string sql = $@"INSERT INTO PROJECT_BASIC_LOG 
                                (PROJECT_NO,
                                PROJECT_NAME,
                                PROJECT_YEAR, 
                                PROJECT_STAGE, 
                                LOG_STATUS,
                                MEMO,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE,
                                MDF_ORG_NAME)
                            OUTPUT INSERTED.LOG_ID
                            VALUES(
                                @PROJECT_NO,
                                @PROJECT_NAME,
                                @PROJECT_YEAR,
                                @PROJECT_STAGE,
                                @LOG_STATUS,
                                @MEMO,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow},
                                @MDF_ORG_NAME)";
            model.MDF_ORG_NAME = user.ORG_NAME;
            return ExecuteQueryFirstOrDefault<int>(sql, model, isSetUSER: true);
        }

        /// <summary>
        /// 新增計劃基本資料歷程檔
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="LOG_ID"></param>
        /// <returns></returns>
        public int InsertProjectBasicHis(string PROJECT_NO, int LOG_ID)
        {
            string sql = $@"INSERT INTO PROJECT_BASIC_HIS (
                                    LOG_ID, PROJECT_NO, PROJECT_NAME, PROJECT_YEAR, PROJECT_TYPE, PROJECT_KIND,
                                    AUTHORIZED_SN, AUTHORIZED_DATE,
                                    CASE_TYPE_SEQ, ASSESS_UNIT, BID_TYPE,
                                    ASSESS_ORGAN_C, MASTER_ORGAN_C, MASTER_UNDERTAKER_C,
                                    PROJECT_LEVEL, EXEC_ORGAN_C, EXEC_UNDERTAKER_C,
                                    BUDGET_HOLD_ORGAN_C,BUDGET_HOLD_UNDERTAKER_C,BUDGET_EXEC_ORGAN_C,BUDGET_EXEC_UNDERTAKER_C,
                                    ASSISTANT_ORGAN_C,ASSISTANT_UNDERTAKER_C,
                                    CP_KIND,RUNWAY_C,
                                    BUDGET_BIGCLASS, BUDGET_LOCAL_SOURCE,BUDGET, BUDGET_REMAIN, BUDGET_HELP,
                                    REASONS, PROPOSEMAN,
                                    CITY_C, TOWN_C, TOWN_M, VILLAGE_C, X_COORD, Y_COORD,
                                    IS_POINTS_CHECK, KML, KML_DEMO,
                                    PROJECT_ORIGIN, PROJECT_GOAL, PROJECT_BENEFIT, PROJECT_FEATURE,
                                    IS_CANCELED, CANCELED_SN,
                                    ALL_JOB, PROFIT, MEMO, NOTES_FOR_SCHEDULE, NOTES_FOR_ACTUAL, NOTES_FOR_BUDGET, 
                                    IS_NEW_CHECK, IS_SCHEDULE_WRITE, IS_SCHEDULE_CHECK,IS_SCHEDULE_EVEN_CHECK,
                                    IS_PROJECT_FINISH, FINISH_DATE,
                                    PROJECT_LAST_DATE, LAST_DATE_HISTORY, LAST_DATE_HISTORY_REASON, EACH_LAST_MONTH_HISTORY, EACH_LAST_MONTH_HISTORY_REASON,
                                    PROJECT_LOCATION, SCHEDULE_MODIFY_COUNT, CANCELED_DATE, NOTE_ID,
                                    SCORE_A, SCORE_ADDPOINT_B, SCORE_B, SCORE_C,
                                    PROJECT_CATEGORY, CONTRACT_FINISH_DATE, CREATE_SOURCE, IS_PROJECT_PUBLIC,
                                    REVOKEREASON, PROMERGESTATUS,
                                    DELAY_FILL_TIME_HISTORY, DELAY_FILL_REASON_HISTORY,
                                    BUDGET_EXEC_YEAR, IS_AUDIT_SETTLED, IS_PROJECT_REVIEW, IS_SPS, SPS_NO,
                                    ACCOUNTING_ITEM, CATEGORY_ORGAN, CREATEDTIME ,
                                    PROJECT_ATT_KIND, REVIEWITEM, PROJECT_STATUS,PROJECT_AW_STATUS,MEMO_EVALUATION,
                                    IS_USER_FTY_DATA,PROMERGE_DATE,MEMO_CHK_POINT,
                                    PROCUREMENT_AMT,TENDER_AWARDING_AMT,
                                    CRT_USER, CRT_DATE, MDF_USER, MDF_DATE) 
                            OUTPUT INSERTED.HIS_ID
                            SELECT
                                    @LOG_ID, PROJECT_NO, PROJECT_NAME, PROJECT_YEAR, PROJECT_TYPE, PROJECT_KIND,
                                    AUTHORIZED_SN, AUTHORIZED_DATE,
                                    CASE_TYPE_SEQ, ASSESS_UNIT, BID_TYPE,
                                    ASSESS_ORGAN_C, MASTER_ORGAN_C, MASTER_UNDERTAKER_C,
                                    PROJECT_LEVEL, EXEC_ORGAN_C, EXEC_UNDERTAKER_C,
                                    BUDGET_HOLD_ORGAN_C,BUDGET_HOLD_UNDERTAKER_C,BUDGET_EXEC_ORGAN_C,BUDGET_EXEC_UNDERTAKER_C,
                                    ASSISTANT_ORGAN_C,ASSISTANT_UNDERTAKER_C,
                                    CP_KIND,RUNWAY_C,
                                    BUDGET_BIGCLASS, BUDGET_LOCAL_SOURCE,BUDGET, BUDGET_REMAIN, BUDGET_HELP,
                                    REASONS, PROPOSEMAN,
                                    CITY_C, TOWN_C, TOWN_M, VILLAGE_C, X_COORD, Y_COORD,
                                    IS_POINTS_CHECK, KML, KML_DEMO,
                                    PROJECT_ORIGIN, PROJECT_GOAL, PROJECT_BENEFIT, PROJECT_FEATURE,
                                    IS_CANCELED, CANCELED_SN,
                                    ALL_JOB, PROFIT, MEMO, NOTES_FOR_SCHEDULE, NOTES_FOR_ACTUAL, NOTES_FOR_BUDGET, 
                                    IS_NEW_CHECK, IS_SCHEDULE_WRITE, IS_SCHEDULE_CHECK,IS_SCHEDULE_EVEN_CHECK,
                                    IS_PROJECT_FINISH, FINISH_DATE,
                                    PROJECT_LAST_DATE, LAST_DATE_HISTORY, LAST_DATE_HISTORY_REASON, EACH_LAST_MONTH_HISTORY, EACH_LAST_MONTH_HISTORY_REASON,
                                    PROJECT_LOCATION, SCHEDULE_MODIFY_COUNT, CANCELED_DATE, NOTE_ID,
                                    SCORE_A, SCORE_ADDPOINT_B, SCORE_B, SCORE_C,
                                    PROJECT_CATEGORY, CONTRACT_FINISH_DATE, CREATE_SOURCE, IS_PROJECT_PUBLIC,
                                    REVOKEREASON, PROMERGESTATUS,
                                    DELAY_FILL_TIME_HISTORY, DELAY_FILL_REASON_HISTORY,
                                    BUDGET_EXEC_YEAR, IS_AUDIT_SETTLED, IS_PROJECT_REVIEW, IS_SPS, SPS_NO,
                                    ACCOUNTING_ITEM, CATEGORY_ORGAN, CREATEDTIME ,
                                    PROJECT_ATT_KIND, REVIEWITEM, PROJECT_STATUS,PROJECT_AW_STATUS,MEMO_EVALUATION,
                                    IS_USER_FTY_DATA,PROMERGE_DATE,MEMO_CHK_POINT,
                                    PROCUREMENT_AMT,TENDER_AWARDING_AMT,
                                    @CRT_USER,{DTNow},@MDF_USER,{DTNow} 
                            from PROJECT_BASIC (noLock) where PROJECT_NO = @PROJECT_NO";

            return ExecuteQueryFirstOrDefault<int>(sql, new
            {
                PROJECT_NO,
                LOG_ID,
                CRT_USER = UserId,
                MDF_USER = UserId
            });
        }
        #endregion

        #region 共用
        /// <summary>
        /// 取得計畫編號流水號
        /// </summary>
        /// <param name="projectNoStart6Char">計畫編號前6碼</param>
        /// <returns></returns>
        public string GetProjectNoSeq(string projectNoStart6Char)
        {
            string sql = @"SELECT ISNULL(MAX(CAST(RIGHT(PROJECT_NO,3) AS INT)),0)+1 AS NUM 
                           FROM PROJECT_BASIC (noLock)
                           WHERE PROJECT_NO LIKE  @projectNoStart6Char + '%'";
            return ExecuteQuery<string>(sql, new { projectNoStart6Char }).FirstOrDefault();
        }

        /// <summary>
        /// 取得計畫審查資料檔
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="PLAN_REVIEW_TYPE"></param>
        /// <returns></returns>
        public async Task<ProjectAuditModel> GetProjectAudit(string PROJECT_NO, string PLAN_REVIEW_TYPE)
        {
            string sql = @" select 
                                REVIEW_RESULT,
                                REVIEW_COMMENTS
                            from PROJECT_AUDIT (nolock)
                            where PROJECT_NO = @PROJECT_NO 
                                and PLAN_REVIEW_TYPE = @PLAN_REVIEW_TYPE
                                and IS_SEND = 0";
            return await ExecuteQueryFirstOrDefaultAsync<ProjectAuditModel>(sql, new { PROJECT_NO, PLAN_REVIEW_TYPE });
        }

        /// <summary>
        /// 新增計畫審查資料檔
        /// </summary>
        /// <param name="model"></param>
        public void InsertProjectAudit(ProjectAuditModel model)
        {
            string sql = $@"INSERT INTO PROJECT_AUDIT(
                                LOG_ID,
                                PROJECT_NO,
                                PLAN_REVIEW_TYPE,
                                REVIEW_RESULT,
                                REVIEW_COMMENTS,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            VALUES(
                                @LOG_ID,
                                @PROJECT_NO,
                                @PLAN_REVIEW_TYPE,
                                @REVIEW_RESULT,
                                @REVIEW_COMMENTS,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 更新計畫審查資料檔
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectAudit(ProjectAuditModel model)
        {
            string sql = $@"update PROJECT_AUDIT 
                                set LOG_ID = @LOG_ID,
                                REVIEW_RESULT = @REVIEW_RESULT,
                                REVIEW_COMMENTS = @REVIEW_COMMENTS,
                                IS_SEND = @IS_SEND,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            where PROJECT_NO = @PROJECT_NO
                                and PLAN_REVIEW_TYPE = @PLAN_REVIEW_TYPE
                                and LOG_ID = 0
                                and IS_SEND = 0";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 取得計畫狀態
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<string> GetProjectStatus(string PROJECT_NO)
        {
            string sql = @" select PROJECT_STATUS 
                            from PROJECT_BASIC (nolock) 
                            where PROJECT_NO = @PROJECT_NO";
            return await ExecuteQueryFirstOrDefaultAsync<string>(sql, new { PROJECT_NO });
        }

        /// <summary>
        /// 新增計畫檢核點歷程檔
        /// </summary>
        /// <param name="model"></param>
        public void InsertProjectCheckpointHis(ProjectHisModel model)
        {
            string sql = $@"Insert into PROJECT_CHECKITEM_HIS
                                (HIS_ID,
                                LOG_ID,
                                PROJECT_NO, 
                                CHECKITEM_SEQ,
                                CHECKITEM_NAME,
                                PROGRESS,
                                ESTIMATED_STARTDATE,
                                ESTIMATED_ENDDATE,
                                ACTUAL_ENDDATE,
                                PCC_ESTIMATED_ENDDATE,
                                PCC_ACTUAL_ENDDATE,
                                IS_DELAY,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            select @HIS_ID,
                                   @LOG_ID,
                                   PROJECT_NO,
                                   CHECKITEM_SEQ, 
                                   CHECKITEM_NAME,
                                   PROGRESS,
                                   ESTIMATED_STARTDATE,
                                   ESTIMATED_ENDDATE,
                                   ACTUAL_ENDDATE,
                                   PCC_ESTIMATED_ENDDATE,
                                   PCC_ACTUAL_ENDDATE,
                                   IS_DELAY, 
                                   @CRT_USER,
                                   {DTNow},
                                   @MDF_USER,
                                   {DTNow} 
                            from PROJECT_CHECKITEM (nolock)
                            where PROJECT_NO = @PROJECT_NO";
            ExecuteCommand(sql, model);

        }

        /// <summary>
        /// 新增計畫經費來源歷程檔
        /// </summary>
        /// <param name="model"></param>
        public void InsertProjectBudgetSourceGHis(ProjectHisModel model)
        {
            string sql = $@"Insert into PROJECT_BUDGET_SOURCE_G_HIS
                                (HIS_ID, LOG_ID,
                                IDENTITY_FIELD, PROJECT_NO,
                                BUDGET_TYPE,
                                PLAN_YEAR,
                                SOURCE_KIND,
                                PLAN_ITEM_C, PLAN_ITEM_L,
                                PLAN_SUBITEM_C,
                                BUDGET_CENTRAL, BUDGET_LOCAL,
                                END_CENTRAL_BALANCE, END_LOCAL_BALANCE,
                                BALANCE_PROJECT_NO, BALANCE_PLAN_ITEM_C, BALANCE_PLAN_SUBITEM_C,
                                EXCEPT_PROJECT_BUDGET,
                                BUDGET_CLASS, BUDGET_PLAN_TYPE,
                                CRT_USER, CRT_DATE, MDF_USER, MDF_DATE)
                            select 
                                @HIS_ID, @LOG_ID,
                                IDENTITY_FIELD, PROJECT_NO,
                                BUDGET_TYPE,
                                PLAN_YEAR,
                                SOURCE_KIND,
                                PLAN_ITEM_C, PLAN_ITEM_L,
                                PLAN_SUBITEM_C,
                                BUDGET_CENTRAL, BUDGET_LOCAL,
                                END_CENTRAL_BALANCE, END_LOCAL_BALANCE,
                                BALANCE_PROJECT_NO, BALANCE_PLAN_ITEM_C, BALANCE_PLAN_SUBITEM_C,
                                EXCEPT_PROJECT_BUDGET,
                                BUDGET_CLASS, BUDGET_PLAN_TYPE,
                                @CRT_USER, {DTNow}, @MDF_USER, {DTNow}

                            from PROJECT_BUDGET_SOURCE_G
                            where PROJECT_NO = @PROJECT_NO";
            ExecuteCommand(sql, model);
        }
        /// <summary>
        /// 計畫建設類別歷程檔
        /// </summary>
        /// <param name="model"></param>
        public void InsertProjectBuildKindHis(ProjectHisModel model)
        {
            string sql = $@"Insert into PROJECT_BUILD_KIND_HIS (
                                HIS_ID,
                                LOG_ID,
                                BUILD_ID,
                                PROJECT_NO,
                                BUILD_KIND_TYPE,
                                BUILD_KIND,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            select 
                                @HIS_ID, 
                                @LOG_ID,
                                BUILD_ID,
                                PROJECT_NO,
                                BUILD_KIND_TYPE,
                                BUILD_KIND,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow}

                            from PROJECT_BUILD_KIND
                            where PROJECT_NO = @PROJECT_NO";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 新增計畫協辦機關歷程檔
        /// </summary>
        /// <param name="model"></param>
        public void InsertProjectAsstOrgHis(ProjectHisModel model)
        {
            string sql = $@"Insert into PROJECT_ASST_ORG_HIS(
                                HIS_ID,
                                LOG_ID,
                                ASST_ID,
                                PROJECT_NO,
                                ASSISTANT_ORGAN_C,
                                ASSISTANT_UNDERTAKER_C,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            select 
                                @HIS_ID,
                                @LOG_ID,
                                ASST_ID,
                                PROJECT_NO,
                                ASSISTANT_ORGAN_C,
                                ASSISTANT_UNDERTAKER_C,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow}
                            from PROJECT_ASST_ORG
                            where PROJECT_NO = @PROJECT_NO";

            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 新增計畫預定實際期程歷程檔
        /// </summary>
        /// <param name="model"></param>
        public void InsertProjectControlExecuteHis(ProjectHisModel model)
        {
            string sql = $@"
                Insert into PROJECT_CONTROL_EXECUTE_HIS (
                    HIS_ID, LOG_ID, PROJECT_NO,
                    CONTROL_DATE1, CONTROL_DATE2, CONTROL_DATE3,
                    CONTROL_DATE4, CONTROL_DATE5, CONTROL_DATE6,
                    CONTROL_PRG1, CONTROL_PRG2, CONTROL_PRG3,
                    CONTROL_PRG4, CONTROL_PRG5, CONTROL_PRG6,
                    EXECUTE_DATE1, EXECUTE_DATE2, EXECUTE_DATE3,
                    EXECUTE_DATE4, EXECUTE_DATE5, EXECUTE_DATE6,
                    PCC_PROJECT_NAME, PCC_PROJECT_NO,
                    FACTORY, FACTORY_CONTACT, FACTORY_TEL, FACTORY_EMAIL,
                    DELAY_CAUSAL, SOLUTION,
                    PRE_PRG, PRG,
                    ENGIN_MONEY, MANAGE_MONEY, DESIGN_MONEY, MONITOR_MONEY, 
                    OTHER_MONEY, OTHER_DESCRIPTION,
                    MATERIAL_MONEY, SELFENGIN_MONEY, 
                    TOTAL_APPOINTED_PAY, TOTAL_ACTUAL_PAY, TOTAL_ACTUAL_COMP,
                    CURRENT_UNPAY, CURRENT_BALANCE,
                    AUDIT_MONEY,
                    LASTMOD_P_A, LASTMOD_T_A, 
                    LASTMOD_P_B, LASTMOD_T_B,
                    LASTMOD_P_C, LASTMOD_T_C,
                    FINAL_ENGIN_MONEY, FINAL_MANAGE_MONEY, FINAL_DESIGN_MONEY,
                    FINAL_MONITOR_MONEY, 
                    FINAL_OTHER_MONEY, FINAL_OTHER_DESCRIPTION,
                    FINAL_MATERIAL_MONEY, FINAL_SELFENGIN_MONEY,
                    EDIT_DATE1, EDIT_DATE2, EDIT_DATE3, 
                    EDIT_DATE4, EDIT_DATE5, EDIT_DATE6, 
                    CONTROL_SDATE1, CONTROL_SDATE2, CONTROL_SDATE3,
                    CONTROL_SDATE4, CONTROL_SDATE5, CONTROL_SDATE6, 
                    PCC_EXEC_ORG_ID,
                    CRT_USER, CRT_DATE, MDF_USER, MDF_DATE,
                    PCC_PROJECT_UID)
                select 
                    @HIS_ID, @LOG_ID, PROJECT_NO,
                    CONTROL_DATE1, CONTROL_DATE2, CONTROL_DATE3,
                    CONTROL_DATE4, CONTROL_DATE5, CONTROL_DATE6,
                    CONTROL_PRG1, CONTROL_PRG2, CONTROL_PRG3,
                    CONTROL_PRG4, CONTROL_PRG5, CONTROL_PRG6,
                    EXECUTE_DATE1, EXECUTE_DATE2, EXECUTE_DATE3,
                    EXECUTE_DATE4, EXECUTE_DATE5, EXECUTE_DATE6,
                    PCC_PROJECT_NAME, PCC_PROJECT_NO,
                    FACTORY, FACTORY_CONTACT, FACTORY_TEL, FACTORY_EMAIL,
                    DELAY_CAUSAL, SOLUTION,
                    PRE_PRG, PRG,
                    ENGIN_MONEY, MANAGE_MONEY, DESIGN_MONEY, MONITOR_MONEY, 
                    OTHER_MONEY, OTHER_DESCRIPTION,
                    MATERIAL_MONEY, SELFENGIN_MONEY, 
                    TOTAL_APPOINTED_PAY, TOTAL_ACTUAL_PAY, TOTAL_ACTUAL_COMP,
                    CURRENT_UNPAY, CURRENT_BALANCE,
                    AUDIT_MONEY,
                    LASTMOD_P_A, LASTMOD_T_A, 
                    LASTMOD_P_B, LASTMOD_T_B,
                    LASTMOD_P_C, LASTMOD_T_C,
                    FINAL_ENGIN_MONEY, FINAL_MANAGE_MONEY, FINAL_DESIGN_MONEY,
                    FINAL_MONITOR_MONEY, 
                    FINAL_OTHER_MONEY, FINAL_OTHER_DESCRIPTION,
                    FINAL_MATERIAL_MONEY, FINAL_SELFENGIN_MONEY,
                    EDIT_DATE1, EDIT_DATE2, EDIT_DATE3, 
                    EDIT_DATE4, EDIT_DATE5, EDIT_DATE6, 
                    CONTROL_SDATE1, CONTROL_SDATE2, CONTROL_SDATE3,
                    CONTROL_SDATE4, CONTROL_SDATE5, CONTROL_SDATE6, 
                    PCC_EXEC_ORG_ID,
                    @CRT_USER, {DTNow}, @MDF_USER, {DTNow},
                    PCC_PROJECT_UID
                from PROJECT_CONTROL_EXECUTE 
                where PROJECT_NO = @PROJECT_NO";

            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 取得是否使用國發會界接資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<bool> GetIsUserFtyData(string PROJECT_NO)
        {
            string sql = @"select 
                                isnull(IS_USER_FTY_DATA, 0)
                            from PROJECT_BASIC (nolock)
                            where PROJECT_NO = @PROJECT_NO";
            return await ExecuteQueryFirstOrDefaultAsync<bool>(sql, new { PROJECT_NO });
        }
        #endregion
    }
}
