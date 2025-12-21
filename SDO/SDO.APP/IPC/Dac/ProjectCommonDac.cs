using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Base.Utils.Models;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class ProjectCommonDac : Dac, IProjectCommonDac
    {
        public ProjectCommonDac(IConnectionControlCenter connectionControlCenter,
           IHttpContextAccessor httpContextAccessor,
           ISqlTrace trace,
           IUserProfile profile,
           IParameterAdaptor ParameterAdaptor,
           IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        #region 相關檔案上傳
        /// <summary>
        /// 取得計畫檔案資料
        /// </summary>
        /// <param name="model">計畫檔案查詢model</param>
        /// <returns>計畫檔案清單</returns>
        public async Task<List<ProjectAttachmentModel>> GetProjectAttachmentList(ProjectAttachmentQueryModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(@"
                SELECT 
                    M2.PROJECT_NO,
	                M2.IDENTITY_FIELD,
	                M2.FILE_KIND,
                    M2.FILE_NAME,
	                M2.FILE_MEMO,
                    M2.FILE_PATH,
	                M3.SET_VALUE AS NAME,
                    M2.FILE_UP_SOURCE,
                    M2.IS_DISPLAY,
                    M2.SOURCE_ID,
                    M2.CRT_DATE,
                    M3.SORT_ORDER
                FROM PROJECT_ATTACHMENT(NOLOCK) M2
                INNER JOIN SET_PARAM(NOLOCK) M3 ON M3.SET_ITEM = 'FILE_KIND' AND M3.SET_TYPE = M2.FILE_KIND
                WHERE M2.PROJECT_NO = @PROJECT_NO");

            if (model.FILE_UP_SOURCE != null)
            {
                sql.AppendLine("AND M2.FILE_UP_SOURCE = @FILE_UP_SOURCE");
            }
            if (model.FILE_KIND != null && model.FILE_KIND.Any())
            {
                sql.AppendLine("AND M2.FILE_KIND in @FILE_KIND");
            }
            if (model.SOURCE_ID != null)
            {
                sql.AppendLine("AND M2.SOURCE_ID = @SOURCE_ID");
            }

            sql.AppendLine("ORDER BY M3.SORT_ORDER");

            string DB = MainDBKey;
            switch ((DBConnection)model.DB)
            {
                case DBConnection.SCDBKey:
                    DB = SCDBKey;
                    break;
                case DBConnection.RISDBKey:
                    DB = RISDBKey;
                    break;
                case DBConnection.IPCDBKey:
                    DB = IPCDBKey;
                    break;
                case DBConnection.INNDBKey:
                    DB = INNDBKey;
                    break;
                case DBConnection.PWSDBKey:
                    DB = PWSDBKey;
                    break;
                case DBConnection.RDDBKey:
                    DB = RDDBKey;
                    break;
            }
            var result = (await ExecuteQueryAsync<ProjectAttachmentModel>(sql.ToString(), model, DB)).ToList();
            return result;
        }

        /// <summary>
        /// 取得非相關檔案上傳的檔案資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<ProjectOtherAttachmentModel>> GetOtherProjectAttachmentList(string PROJECT_NO)
        {
            string sql = @"
                SELECT main.PROJECT_NO
	                ,main.IDENTITY_FIELD
	                ,main.FILE_KIND
                    ,main.FILE_NAME
                    ,main.SOURCE_ID
                    ,(CASE 
	                    WHEN FILE_KIND = '02' THEN CONCAT(dbo.FN_ToTWDate(main.CRT_DATE, 1), ' ', main.FILE_NAME)
		                WHEN FILE_KIND IN ('05', '06') THEN CONCAT(budget.PLAN_YEAR, '年度', cpi.PLAN_ITEM_NAME, '核定文件')
	                    WHEN FILE_KIND IN ('07', '08') THEN CONCAT(dbo.FN_ToTWDate(fact.FFDATE, 1), fileKindDesc.MEMO)
                        WHEN FILE_KIND = '09' THEN CONCAT(dbo.FN_ToTWDate(main.CRT_DATE, 1), fileKindDesc.SET_VALUE)
	                    WHEN FILE_KIND = '16' THEN main.FILE_NAME
	                    WHEN FILE_KIND IN ('17', '18') THEN CONCAT(dbo.FN_ToTWDate(mergelog.PROMERGE_DATE, 1), fileKindDesc.MEMO)
                        WHEN FILE_KIND = '19' THEN CONCAT(dbo.FN_ToTWDate(adj.APPRV_DATE, 1), fileKindDesc.MEMO)
                    END) AS FILE_MEMO
                    ,(CASE
	                    WHEN FILE_KIND IN ('10', '11', '12', '13', '21', '22', '23') THEN 'SCHE'
	                    ELSE FILE_KIND
                    END) AS FILE_TYPE
                FROM PROJECT_ATTACHMENT (NOLOCK) main
                LEFT JOIN PROJECT_BUDGET_SOURCE_G (NOLOCK) budget 
                    ON main.SOURCE_ID = budget.IDENTITY_FIELD AND main.FILE_KIND IN ('05', '06')
                LEFT JOIN PROJECT_FACT_FINDING (NOLOCK) fact 
                    ON main.SOURCE_ID = fact.SEQ AND main.FILE_KIND IN ('07', '08')
                LEFT JOIN PROJECT_MERGE_LOG (NOLOCK) mergelog 
                    ON main.SOURCE_ID = mergelog.SEQ AND main.FILE_KIND IN ('17', '18')
                LEFT JOIN PROJECT_BASIC_ADJ (NOLOCK) adj 
                    ON main.SOURCE_ID = adj.PROJ_ADJ_ID AND adj.DEL_FLG = 0 AND main.FILE_KIND IN ('09', '10', '11', '12', '13', '19', '21', '22', '23')
                LEFT JOIN SET_PARAM (NOLOCK) fileKindDesc 
                    ON fileKindDesc.SET_ITEM = 'FILE_KIND' AND fileKindDesc.SET_TYPE = main.FILE_KIND
                LEFT JOIN CODE_PLAN_ITEM (NOLOCK) cpi 
                    ON cpi.PLAN_ITEM_ID = budget.PLAN_ITEM_C
                WHERE main.PROJECT_NO = @PROJECT_NO
                    AND ( 
	                    (adj.PROJECT_AW_STATUS IS NULL AND main.FILE_KIND IN ('02', '05', '06', '07', '08', '16', '17', '18'))
                        OR (PROJECT_AW_STATUS IN ('A05', 'B05', 'W05'))
                    )";
            return (await ExecuteQueryAsync<ProjectOtherAttachmentModel>(sql, new { PROJECT_NO })).ToList();
        }

        /// <summary>
        /// 取得單一計畫檔案資料(下載檔案用)
        /// </summary>
        /// <param name="IDENTITY_FIELD"></param>
        /// <returns></returns>
        public async Task<ProjectAttachmentModel> GetProjectAttachment(int IDENTITY_FIELD , int DBKEY)
        {
            string sql = @"SELECT IDENTITY_FIELD, FILE_NAME, FILE_PATH
                           FROM PROJECT_ATTACHMENT (NOLOCK)
                           WHERE IDENTITY_FIELD = @IDENTITY_FIELD";
            string DB = MainDBKey;
            switch ((DBConnection)DBKEY)
            {
                case DBConnection.SCDBKey:
                    DB = SCDBKey;
                    break;
                case DBConnection.RISDBKey:
                    DB = RISDBKey;
                    break;
                case DBConnection.IPCDBKey:
                    DB = IPCDBKey;
                    break;
                case DBConnection.INNDBKey:
                    DB = INNDBKey;
                    break;
                case DBConnection.PWSDBKey:
                    DB = PWSDBKey;
                    break;
                case DBConnection.RDDBKey:
                    DB = RDDBKey;
                    break;
            }

            return await ExecuteQueryFirstOrDefaultAsync<ProjectAttachmentModel>(sql, new { IDENTITY_FIELD } ,DB);
        }

        /// <summary>
        /// 取得多筆計畫檔案資料(下載zip檔案用)
        /// </summary>
        /// <param name="IDENTITY_FIELDs"></param>
        /// <returns></returns>
        public async Task<List<ProjectAttachmentModel>> GetProjectAttachment(List<int> IDENTITY_FIELDs)
        {
            string sql = @"SELECT IDENTITY_FIELD, FILE_NAME, FILE_PATH, param.SET_VALUE AS NAME, param.SORT_ORDER
                           FROM PROJECT_ATTACHMENT (NOLOCK) main
                           INNER JOIN SET_PARAM (NOLOCK) param 
                               ON param.SET_ITEM = 'FILE_KIND' AND param.SET_TYPE = main.FILE_KIND
                           WHERE IDENTITY_FIELD IN @IDENTITY_FIELDs
                           ORDER BY param.SORT_ORDER";
            return (await ExecuteQueryAsync<ProjectAttachmentModel>(sql, new { IDENTITY_FIELDs })).ToList();
        }

        /// <summary>
        /// 新增計畫檔案資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int InsertProjectAttachment(ProjectAttachmentModel model)
        {
            string sql = $@"INSERT INTO PROJECT_ATTACHMENT
                                (PROJECT_NO,
                                FILE_KIND,
                                FILE_NAME,
                                FILE_MEMO,
                                FILE_PATH,
                                FILE_UP_SOURCE,
                                IS_DISPLAY,
                                SOURCE_ID,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            OUTPUT INSERTED.IDENTITY_FIELD
                            VALUES
                                (@PROJECT_NO,
                                @FILE_KIND,
                                @FILE_NAME,
                                @FILE_MEMO,
                                @FILE_PATH,
                                @FILE_UP_SOURCE,
                                @IS_DISPLAY,
                                @SOURCE_ID,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            string DB = MainDBKey;
            switch ((DBConnection)model.DB)
            {
                case DBConnection.SCDBKey:
                    DB = SCDBKey;
                    break;
                case DBConnection.RISDBKey:
                    DB = RISDBKey;
                    break;
                case DBConnection.IPCDBKey:
                    DB = IPCDBKey;
                    break;
                case DBConnection.INNDBKey:
                    DB = INNDBKey;
                    break;
                case DBConnection.PWSDBKey:
                    DB = PWSDBKey;
                    break;
                case DBConnection.RDDBKey:
                    DB = RDDBKey;
                    break;
            }

            return ExecuteQuery<int>(sql, model, isSetUSER: true, DB: DB).FirstOrDefault();
        }

        /// <summary>
        /// 修改計畫檔案資料
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectAttachment(ProjectAttachmentModel model)
        {
            string sql = $@"UPDATE PROJECT_ATTACHMENT
                            SET FILE_KIND = @FILE_KIND,
                                FILE_NAME = @FILE_NAME,
                                FILE_MEMO = @FILE_MEMO,
                                IS_DISPLAY = @IS_DISPLAY,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE IDENTITY_FIELD = @IDENTITY_FIELD";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 刪除計畫檔案資料
        /// </summary>
        /// <param name="IDENTITY_FIELD"></param>
        public void DeleteProjectAttachment(int IDENTITY_FIELD)
        {
            string sql = $@"DELETE PROJECT_ATTACHMENT WHERE IDENTITY_FIELD = @IDENTITY_FIELD";
            ExecuteCommand(sql, new { IDENTITY_FIELD });
        }

        public void DeleteProjectAttachment(int IDENTITY_FIELD, int DBKey)
        {
            string sql = $@"DELETE PROJECT_ATTACHMENT WHERE IDENTITY_FIELD = @IDENTITY_FIELD";
            string DB = MainDBKey;
            switch ((DBConnection)DBKey)
            {
                case DBConnection.SCDBKey:
                    DB = SCDBKey;
                    break;
                case DBConnection.RISDBKey:
                    DB = RISDBKey;
                    break;
                case DBConnection.IPCDBKey:
                    DB = IPCDBKey;
                    break;
                case DBConnection.INNDBKey:
                    DB = INNDBKey;
                    break;
                case DBConnection.PWSDBKey:
                    DB = PWSDBKey;
                    break;
                case DBConnection.RDDBKey:
                    DB = RDDBKey;
                    break;
            }
            ExecuteCommand(sql, new { IDENTITY_FIELD }, DB);
        }

        /// <summary>
        /// 刪除計畫檔案資料
        /// </summary>
        /// <param name="model"></param>
        public void DeleteProjectAttachmentAll(ProjectAttachmentModel model)
        {
            string sourceIdConditions = model.SOURCE_ID.HasValue ? " SOURCE_ID = @SOURCE_ID" : " SOURCE_ID IS NULL";
            string sql = $"DELETE PROJECT_ATTACHMENT WHERE PROJECT_NO = @PROJECT_NO AND FILE_KIND = @FILE_KIND AND {sourceIdConditions}";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 刪除再新增計畫檔案資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int MdfProjectAttachment(ProjectAttachmentModel model)
        {

            string sql = $@"
                            -- 再新增
                            INSERT INTO PROJECT_ATTACHMENT
                                (PROJECT_NO,
                                FILE_KIND,
                                FILE_NAME,
                                FILE_MEMO,
                                FILE_PATH,
                                FILE_UP_SOURCE,
                                IS_DISPLAY,
                                SOURCE_ID,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            OUTPUT INSERTED.IDENTITY_FIELD
                            VALUES
                                (@PROJECT_NO,
                                @FILE_KIND,
                                @FILE_NAME,
                                @FILE_MEMO,
                                @FILE_PATH,
                                @FILE_UP_SOURCE,
                                @IS_DISPLAY,
                                @SOURCE_ID,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";


            return ExecuteQuery<int>(sql, model, isSetUSER: true).FirstOrDefault();
        }
        #endregion

        #region 參考資料
        /// <summary>
        /// 取得參考資料
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProjectAttachmentModel>> GetRefFile()
        {
            string sql = @"SELECT IDENTITY_FIELD, FILE_NAME
                           FROM PROJECT_ATTACHMENT (NOLOCK)
                           WHERE PROJECT_NO = '0'";
            return (await ExecuteQueryAsync<ProjectAttachmentModel>(sql)).ToList();
        }
        #endregion

        #region 計畫參數值對應資料
        /// <summary>
        /// 取得計畫參數值對應資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="SET_ITEM"></param>
        /// <param name="SOURCE_ID"></param>
        /// <returns></returns>
        public async Task<List<ProjectMappingDataModel>> GetProjectMappingData(string PROJECT_NO, string SET_ITEM, string SOURCE_ID = null)
        {
            string sql = @" select PROJECT_NO,
                                   SET_ITEM,
                                   SET_TYPE,
                                   SOURCE_ID
                            from PROJECT_MAPPING_DATA (nolock)
                            where PROJECT_NO = @PROJECT_NO 
                                and SET_ITEM = @SET_ITEM";
            if (!string.IsNullOrEmpty(SOURCE_ID))
            {
                sql += " and SOURCE_ID = @SOURCE_ID";
            }
            return (await ExecuteQueryAsync<ProjectMappingDataModel>(sql, new { PROJECT_NO, SET_ITEM, SOURCE_ID })).ToList();
        }

        /// <summary>
        /// 刪除計畫參數值對應資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="SET_ITEM"></param>
        /// <param name="SOURCE_ID"></param>
        public void DeleteProjectMappingData(string PROJECT_NO, string SET_ITEM, string SOURCE_ID)
        {
            string sql = @"delete PROJECT_MAPPING_DATA where PROJECT_NO = @PROJECT_NO and SET_ITEM = @SET_ITEM and SOURCE_ID = @SOURCE_ID ";

            ExecuteCommand(sql, new { PROJECT_NO, SET_ITEM, SOURCE_ID });
        }

        /// <summary>
        /// 新增計畫參數值對應資料
        /// </summary>
        /// <param name="models"></param>
        public void InsertProjectMappingData(List<ProjectMappingDataModel> models)
        {
            string sql = $@"Insert into PROJECT_MAPPING_DATA 
                                (PROJECT_NO,
                                SET_ITEM,
                                SET_TYPE,
                                SOURCE_ID,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            VALUES(
                                @PROJECT_NO,
                                @SET_ITEM,
                                @SET_TYPE,
                                @SOURCE_ID,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            ExecuteCommand(sql, models);
        }
        #endregion

        /// <summary>
        /// 取得當期計畫填報周期
        /// </summary>
        /// <returns></returns>
        public async Task<ProjectFillCycleModel> GetCurrentCycleData()
        {
            string sql = @"select TOP 1 SEQ,
                                PROJECT_YEAR,
                                PROJECT_MONTH,
                                FILL_START_DATE,
                                FILL_END_DATE 
                            from PROJECT_FILL_CYCLE (nolock)
                            order by SEQ desc";

            return await ExecuteQueryFirstOrDefaultAsync<ProjectFillCycleModel>(sql);
        }

        /// <summary>
        /// 取得計畫填報周期(多筆)
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProjectFillCycleModel>> GetCycleData()
        {
            string sql = @"select SEQ,
                                PROJECT_YEAR,
                                PROJECT_MONTH,
                                FILL_START_DATE,
                                FILL_END_DATE 
                            from PROJECT_FILL_CYCLE (nolock)
                            order by SEQ desc";
            return (await ExecuteQueryAsync<ProjectFillCycleModel>(sql)).ToList();
        }

        /// <summary>
        /// 取得章節表前端頁面路徑
        /// </summary>
        /// <param name="CHAPTER_ID"></param>
        /// <returns></returns>
        public async Task<string> GetChapterPath(string CHAPTER_ID)
        {
            string sql = @"select SOURCE_PATH 
                           from PROJECT_CHAPTER (nolock)　
                           where CHAPTER_ID = @CHAPTER_ID";
            return await ExecuteQueryFirstOrDefaultAsync<string>(sql, new { CHAPTER_ID });
        }

        #region 年終考核
        /// <summary>
        /// 取得年終考核
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<ProjectFillYearAssModel> GetProjectFillYearAss(string PROJECT_NO)
        {
            string sql = @"SELECT M1.PROJECT_NO
	                            ,M1.PROJECT_NAME
	                            ,M1.PROJECT_STATUS
	                            ,M1.PROJECT_YEAR
	                            ,M1.CREATEDTIME
	                            ,M1.PROJECT_LAST_DATE
                                ,DATEDIFF(MONTH, M1.CREATEDTIME, M1.PROJECT_LAST_DATE) AS ExMonth
	                            ,M1.SCORE_A
                                ,CASE WHEN m.SCHE_M_CNT IS NULL THEN 0 ELSE m.SCHE_M_CNT END AS SCHE_M_CNT
								,CASE WHEN y.SCHE_Y_CNT IS NULL THEN 0 ELSE y.SCHE_Y_CNT END AS SCHE_Y_CNT
	                            ,M1.CP_KIND
	                            ,M2.CHECKPOINT_CLASS
                                ,M3.ACTUAL_PAY
	                            ,M3.UNPAY
	                            ,M3.BALANCE
                                ,M4.CONTROL_DATE1
                                ,M1.FINISH_DATE
                            FROM PROJECT_BASIC (NOLOCK) M1
                            LEFT JOIN CODE_CHECKPOINT (NOLOCK) M2 on M1.RUNWAY_C = M2.CHECKPOINT_CLASS_ID
                            LEFT JOIN (
                                SELECT  
	                                PROJECT_NO, ACTUAL_PAY, UNPAY, BALANCE ,
	                                ROW_NUMBER() OVER ( PARTITION BY PROJECT_NO ORDER BY IDENTITY_FIELD DESC ) AS RN
                                FROM PROJECT_PAYMENT (NOLOCK)
                            ) M3 ON M1.PROJECT_NO = M3.PROJECT_NO AND RN = 1
                            LEFT JOIN PROJECT_CONTROL_EXECUTE (NOLOCK) M4 on M1.PROJECT_NO = M4.PROJECT_NO
                            LEFT JOIN (
								SELECT PROJECT_NO, SCHE_TYPE, COUNT(SCHE_TYPE) AS SCHE_M_CNT
								FROM PROJECT_BASIC_ADJ (NOLOCK)
								WHERE SCHE_TYPE = 'M' AND DEL_FLG = 0 AND PROJECT_AW_STATUS = 'B05'
								GROUP BY SCHE_TYPE, PROJECT_NO
							) m ON M1.PROJECT_NO = m.PROJECT_NO
                            LEFT JOIN (
								SELECT PROJECT_NO, SCHE_TYPE, COUNT(SCHE_TYPE) AS SCHE_Y_CNT
								FROM PROJECT_BASIC_ADJ (NOLOCK)
								WHERE SCHE_TYPE = 'Y' AND DEL_FLG = 0  AND PROJECT_AW_STATUS = 'B05'
								GROUP BY SCHE_TYPE, PROJECT_NO
							) y ON M1.PROJECT_NO = y.PROJECT_NO
                            WHERE M1.PROJECT_NO = @PROJECT_NO";
            return await ExecuteQueryFirstOrDefaultAsync<ProjectFillYearAssModel>(sql, new { PROJECT_NO });
        }
        #endregion

        #region 檢查同一計畫編號相同FILE_KIND的PROJECT_ATTACHMENT檔名不能重複
        /// <summary>
        /// 取得同一計畫編號FILE_KIND所有檔案
        /// </summary>
        /// <param name="PROJECT_NO">計畫編號</param>
        /// <param name="FILE_KIND">檔案類型</param>
        /// <param name="IDENTITY_FIELD">排除不查的檔案ID</param>
        /// <param name="PROJ_ADJ_ID">調整撤銷流水號</param>
        /// <returns></returns>
        public List<(string fileName, string fileKind)> GetFileNameByFileKind(string PROJECT_NO, List<string> FILE_KIND, List<int> IDENTITY_FIELD, int? PROJ_ADJ_ID, int DBKey)
        {
            //有PROJ_ADJ_ID以調整撤銷流水號跟檔案類型判斷，其餘以計畫編號跟檔案類型判斷
            string sql = $@"SELECT FILE_NAME, FILE_KIND
                            FROM PROJECT_ATTACHMENT (NOLOCK)
                            WHERE 1=1";

            if ( FILE_KIND.Any() && FILE_KIND.All(kind => kind != null))
            {
                sql += " AND FILE_KIND IN @FILE_KIND";
            }

            if (PROJ_ADJ_ID == null)
            
            {
                sql += " AND PROJECT_NO = @PROJECT_NO";
            }
            else
            {
                sql += " AND SOURCE_ID = @PROJ_ADJ_ID";
            }
            if (IDENTITY_FIELD.Any())
            {
                sql += " AND IDENTITY_FIELD NOT IN @IDENTITY_FIELD";
            }
            string DB = MainDBKey;
            switch ((DBConnection)DBKey)
            {
                case DBConnection.SCDBKey:
                    DB = SCDBKey;
                    break;
                case DBConnection.RISDBKey:
                    DB = RISDBKey;
                    break;
                case DBConnection.IPCDBKey:
                    DB = IPCDBKey;
                    break;
                case DBConnection.INNDBKey:
                    DB = INNDBKey;
                    break;
                case DBConnection.PWSDBKey:
                    DB = PWSDBKey;
                    break;
                case DBConnection.RDDBKey:
                    DB = RDDBKey;
                    break;
            }
            return ExecuteQuery<(string fileName, string fileKind)>(sql, new { PROJECT_NO, FILE_KIND, IDENTITY_FIELD, PROJ_ADJ_ID }, DB).ToList();
        }
        #endregion

        /// <summary>
        /// 取得已使用的代碼清單
        /// </summary>
        /// <param name="SET_ITEM">代碼類別</param>
        /// <returns></returns>
        public async Task<List<string>> GetUsedCode(string SET_ITEM)
        {
            string sql = @"select DISTINCT SET_TYPE 
                            from PROJECT_MAPPING_DATA (nolock) 
                            where SET_ITEM = @SET_ITEM";
            return (await ExecuteQueryAsync<string>(sql, new { SET_ITEM })).ToList();
        }

        /// <summary>
        /// 取得計畫落後原因的落後項目代碼清單
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetDelayClasses()
        {
            string sql = @"select distinct DELAY_CLASS_C 
                            from PROJECT_DELAY_CAUSAL (nolock)
                            where DELAY_CLASS_C is not null";
            return (await ExecuteQueryAsync<string>(sql)).ToList();
        }

        /// <summary>
        /// 取得計畫落後原因的落後項目清單
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetDelaySubClasses()
        {
            string sql = @"select distinct DELAY_SUBCLASS_C 
                            from PROJECT_DELAY_CAUSAL (nolock)
                            where DELAY_SUBCLASS_C is not null";
            return (await ExecuteQueryAsync<string>(sql)).ToList();
        }

        /// <summary>
        /// 取得計畫經費來源的預算編號清單
        /// </summary>
        /// <param name="LEVEL_MARK">預算來源類別</param>
        /// <returns></returns>
        public async Task<List<string>> GetPlanItems(string LEVEL_MARK)
        {
            string column = LEVEL_MARK == "1" ? "PLAN_ITEM_C" : "PLAN_ITEM_L";
            string sql = $@"select distinct
                                {column}
                            from PROJECT_BUDGET_SOURCE_G (nolock)
                            where isnull({column}, '') != ''";
            return (await ExecuteQueryAsync<string>(sql)).ToList();
        }

        /// <summary>
        /// 取得機關窗口資料
        /// </summary>
        /// <param name="orgIds"></param>
        /// <returns></returns>
        public async Task<List<DeptContactModel>> GetDeptContactByOrg(List<string> orgIds)
        {
            string sql = @" select DC_ID,
                                ORGAN,
                                SOURCE,
                                CONTACT,
                                EMAIL
                            from DEPT_CONTACT (nolock)
                            where ORGAN in @orgIds";

            return (await ExecuteQueryAsync<DeptContactModel>(sql, new { orgIds })).ToList();
        }

        /// <summary>
        /// 取得SCUser信箱資料
        /// </summary>
        /// <param name="UserIds"></param>
        /// <returns></returns>
        public async Task<List<SCContactModel>> GetSCContactData(List<string> UserIds)
        {
            string sql = @"select USR_ID,
                                  USR_NAME,
                                  USR_EMAIL 
                            from SCUSERM 
                            where USR_ID in @UserIds";
            return (await ExecuteQueryAsync<SCContactModel>(sql, new { UserIds }, SCDBKey)).ToList();
        }

        /// <summary>
        /// 取得郵件範本替換參數資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<MailTemplateParamModel> GetMailTemplateParam(string PROJECT_NO)
        {
            string sql = $@" select 
                                M1.PROJECT_NO,
                                M1.PROJECT_NAME,
                                [dbo].[FN_GetOuName](M1.EXEC_ORGAN_C,3) as EXEC_ORGAN_NAME , 
                                scUser.USR_NAME as EXEC_UNDERTAKER_NAME,
                                scUser.USR_CUSTOM1 as EXEC_UNDERTAKER_TEL,
                                M2.REAL_CONTACT,
                                M2.REAL_TEL,
                                (select USR_NAME from {SC30_M}.SCUSERM where USR_ID = '{UserId}') as RDEC_NAME,
                                (select USR_CUSTOM1 from {SC30_M}.SCUSERM where USR_ID = '{UserId}') as RDEC_TEL
                            from PROJECT_BASIC M1
                            left join PROJECT_CONTROL_EXECUTE M2
                                on M1.PROJECT_NO = M2.PROJECT_NO
                            left join {SC30_M}.SCUSERM scUser
                                on M1.EXEC_UNDERTAKER_C = scUser.USR_ID
                            where M1.PROJECT_NO = @PROJECT_NO";

            return await ExecuteQueryFirstOrDefaultAsync<MailTemplateParamModel>(sql, new { PROJECT_NO });

        }

        /// <summary>
        /// 取得落後原因類型
        /// </summary>
        /// <param name="PROJECT_NO">計畫編號</param>
        /// <param name="FILL_END_DATE">填報週期迄</param>
        /// <returns></returns>
        public async Task<string> GetDelayKind(string PROJECT_NO, DateTime FILL_END_DATE)
        {
            string sql = "select dbo.FN_GET_DELAY_TYPE(@PROJECT_NO, null, @FILL_END_DATE)";
            return (await ExecuteQueryAsync<string>(sql, new { PROJECT_NO, FILL_END_DATE })).FirstOrDefault();
        }

        /// <summary>
        /// 移除當月工程進度
        /// </summary>
        /// <param name="projectNos"></param>
        public void DeleteLatestProjectEngProgess(List<string> projectNos)
        {
            string sql = @"delete PROJECT_ENGINEERING_PROGRESS 
						   where PROJECT_NO in @projectNos 
							   and YEAR = (select top 1 PROJECT_YEAR from PROJECT_FILL_CYCLE order by SEQ desc)
							   and MONTH = (select top 1 PROJECT_MONTH from PROJECT_FILL_CYCLE order by SEQ desc)";
            ExecuteCommand(sql, new { projectNos });
        }

        /// <summary>
        /// 移除當月落後原因
        /// </summary>
        /// <param name="projectNos"></param>
        public void DeleteLatestDelayCausal(List<string> projectNos)
        {
            string sql = @"delete PROJECT_DELAY_CAUSAL 
						   where PROJECT_NO in @projectNos
							   and DATA_YEAR  = (select top 1 PROJECT_YEAR from PROJECT_FILL_CYCLE order by SEQ desc)
							   and DATA_MONTH = (select top 1 PROJECT_MONTH from PROJECT_FILL_CYCLE order by SEQ desc)";
            ExecuteCommand(sql, new { projectNos });
        }
    }
}
