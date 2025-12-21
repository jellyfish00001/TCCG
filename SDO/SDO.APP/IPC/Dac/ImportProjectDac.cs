using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Models;
using SDO.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class ImportProjectDac : Dac, IImportProjectDac
    {
        public ImportProjectDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor, 
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor,trace,profile ,ParameterAdaptor,configuration)
        {
        }

        #region 先期計畫匯入
        /// <summary>
        /// 取得先期計畫資料列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<PWSSDPlanGridModel>> QueryPWSSDPlanList(ImportProjectQueryModel model)
        {
            string sql = $@" SELECT A.PlanId,
                                    A.PlanYear,
                                    A.PlanName,
                                    B.OU_NAME, --主管機關
                                    A.PlanTotMoney,
                                    -- 提報單位(提報機關)
							        dbo.FN_GetOuName(A.CreateUnitOuId,3) + '(' + dbo.FN_GetOuName(A.CreateOrgOuId,3) + ')' as ExecUnitName, 
							        -- 執行機關(提報機關的一級機關名稱)
							        dbo.FN_GetOuName(CASE execOrg.OU_KIND WHEN 2 THEN execOrg.PARENT_OU_ID ELSE execOrg.OU_ID END ,3) as ExecOrgName, 
                                    IIF(C.CREATE_SOURCE > 0,1,0) as IsImport,
                                    CAST(YEAR(A.PlanStartDate)-1911 AS VARCHAR(3)) + '/' 
                                        + right('00'+cast(CAST(MONTH(A.PlanStartDate) AS VARCHAR(3))as varchar),2)
                                        + ' ~ ' 
                                        + CAST(YEAR(A.PlanEndDate)-1911 AS VARCHAR(3)) + '/' 
                                        + right('00'+cast(CAST(MONTH(A.PlanEndDate) AS VARCHAR(3))as varchar),2) AS PlanDate,
                                    CASE D.SendStatus WHEN '0' THEN '研擬中' WHEN '1' THEN '審核中' END AS SendStatus
                            FROM {RIS_M}.PWSSDPlanMain A
                            LEFT JOIN {SC30_M}.SCORG_UNITM B 
                                on A.OU_ID =B.OU_ID
                            LEFT JOIN PROJECT_BASIC C 
                                on A.PlanId =C.CREATE_SOURCE
                            LEFT JOIN {RIS_M}.PWSSDAssignment D 
                                on D.OU_ID = A.OU_ID and D.PlanYear =  A.PlanYear
                            LEFT JOIN {SC30_M}.IPC_SCORG_UNITV execOrg
								on execOrg.OU_ID = A.CreateOrgOuId 

                            WHERE PlanKind='1' " ;

            // 年度
            if (!string.IsNullOrEmpty(model.planYear))
                sql += " and A.PlanYear = @planYear";

            // 主管機關
            if (!string.IsNullOrEmpty(model.organ))
                sql += " and A.OU_ID = @organ";
            
            // 審核狀態
            if (!string.IsNullOrEmpty(model.isSend))
                sql += " and D.SendStatus = @isSend";
            // 已匯入/未匯入
            if (!string.IsNullOrEmpty(model.isImport))
            {
                if (model.isImport == "0")
                    sql += @" and C.CREATE_SOURCE IS NULL";
                else if(model.isImport == "1")
                    sql += @" and C.CREATE_SOURCE IS NOT NULL";
            }

            return (await ExecuteQueryAsync<PWSSDPlanGridModel>(sql,model)).ToList();
        }

        /// <summary>
        /// 取得先期計畫資料
        /// </summary>
        /// <param name="PlanId"></param>
        /// <returns></returns>
        public async Task<PWSSDPlanMainModel>GetPWSSDPlan(int PlanId)
        {
            string sql = $@"SELECT  PlanId,
                                    planMain.PlanYear,
                                    planMain.PlanName,
                                    planMain.OU_ID,
                                    planMain.PlanDateType,
									-- 提報機關的一級機關
									CASE execOrg.OU_KIND WHEN 2 THEN execOrg.PARENT_OU_ID ELSE execOrg.OU_ID END as ExecOrgId,
                                    planMain.ExplainNecessity,
                                    planMain.ExplanBasicInfo,
									planMain.CreatedUserId
                            FROM {RIS_M}.PWSSDPlanMain  planMain --DBLink不能加nolock
							LEFT JOIN {SC30_M}.IPC_SCORG_UNITV execOrg
								on execOrg.OU_ID = planMain.CreateOrgOuId 
                            WHERE planMain.PlanId = @PlanId";
            
            return await ExecuteQueryFirstOrDefaultAsync<PWSSDPlanMainModel>(sql, new { PlanId },MainDBKey);
        }

        /// <summary>
        /// 取得計畫預定完成期限
        /// </summary>
        /// <param name="PlanId"></param>
        /// <returns></returns>
        public async Task<DateTime?> GetProjectLastDate(int PlanId)
        {
            string sql = $@"SELECT top 1 EstimatedEndDate 
                            FROM {RIS_M}.CustomCheckItemDate --DBLink不能加nolock
                            WHERE PlanId = @PlanId　order by EstimatedEndDate desc";

            return  await ExecuteQueryFirstOrDefaultAsync<DateTime?>(sql, new { PlanId });
        }

        /// <summary>
        /// 匯入先期計畫檢核點日期
        /// </summary>
        /// <param name="PlanId"></param>
        /// <param name="ProjectNo"></param>
        public void InsertCustomCheckItemDateByPlanId(int PlanId,string ProjectNo)
        {
            string sql = $@"INSERT INTO PROJECT_CHECKITEM 
                                (PROJECT_NO,
                                 CHECKITEM_SEQ,
                                 ESTIMATED_STARTDATE,
                                 ESTIMATED_ENDDATE, 
							     CRT_USER,
                                 CRT_DATE, 
                                 MDF_USER,
                                 MDF_DATE)
	                                SELECT
		                                @ProjectNo as PROJECT_NO,
		                                CheckItemSeq as CHECKITEM_SEQ,
		                                EstimatedStartDate as ESTIMATED_STARTDATE,
		                                EstimatedEndDate as ESTIMATED_ENDDATE,
								        @CRT_USER as CRT_USER,
		                                {DTNow} as CRT_DATE,
		                                @MDF_USER as MDF_USER,
								        {DTNow} as MDF_DATE
	                                FROM {RIS_M}.CustomCheckItemDate --DBLink不能加nolock
	                                WHERE PlanId = @PlanId";
            ExecuteCommand(sql, new { 
                PlanId,
                ProjectNo,
                CRT_USER = UserId,
                MDF_USER = UserId});
        }

        /// <summary>
        /// 先期計畫匯入 - 新增計畫基本資料
        /// </summary>
        /// <param name="ProjectNo"></param>
        /// <param name="ProjectLastDate"></param>
        /// <param name="model"></param>
        public void InsertProjectBasicByPWSSD(string ProjectNo, DateTime? ProjectLastDate ,PWSSDPlanMainModel model)
        {
            string sql = $@"INSERT INTO PROJECT_BASIC(
                                PROJECT_NO,
                                PROJECT_YEAR,
                                PROJECT_NAME,
                                ASSESS_ORGAN_C,
                                CATEGORY_ORGAN, 
                                MASTER_ORGAN_C, 
                                EXEC_ORGAN_C, 
                                PROJECT_BENEFIT, 
                                ALL_JOB, 
                                MEMO, 
                                PROJECT_LAST_DATE, 
                                IS_NEW_CHECK, 
                                IS_SCHEDULE_WRITE, 
                                IS_SCHEDULE_CHECK, 
                                IS_SCHEDULE_EVEN_CHECK,
                                PROJECT_CATEGORY, 
                                CREATE_SOURCE, 
                                CREATEDTIME,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER, 
                                MDF_DATE) 
                            VALUES (
                                @ProjectNo,
                                @PlanYear,
                                @PlanName,
                                '380000000A',           --列管機關 寫死 因為專案只給桃園市政府用了 380000000A
                                '380000000A',           --列管機關 寫死 因為專案只給桃園市政府用了 380000000A
                                @OU_ID,                 --主管機關
                                @ExecOrgId,             --執行機關 (先期:提報局處)
                                @ExplainNecessity,      --計畫效益 (先期:一、(一)說明計畫之必要性、亮點及效益)
                                @ExplanBasicInfo,       --計畫效益 (先期:一、(二)說明計畫之基本資料)
                                @PlanDateType,          --備註(先期:計畫性質)
                                @ProjectLastDate,       --預定完成期限
                                '0',                    --基本資料研擬狀態
                                '0',                    --基本資料研擬狀態
                                '0',                    --基本資料研擬狀態
                                '0',                    --基本資料研擬狀態
                                'G00',                  --列管類別 = 重大
                                @PlanId,                --資料來源 (儲存先期PlanId，對應重大PROJECT_NO與先期PlanId)    
                                {DTNow},
                                @CRT_USER,
                                {DTNow},
                                @CreatedUserId,         --新增人員(紀錄先期原本新增人員) -- 待確認是否需要改為當前使用者
                                {DTNow})";

            ExecuteCommand(sql, new
            {
                ProjectNo,
                model.PlanYear,
                model.PlanName,
                model.OU_ID,
                model.ExecOrgId,
                model.ExplainNecessity,
                model.ExplanBasicInfo,
                model.PlanDateType,
                ProjectLastDate,
                model.PlanId,
                CRT_USER = UserId,
                model.CreatedUserId
            });
        }

        /// <summary>
        /// 先期計畫匯入 - 新增計畫經費來源
        /// </summary>
        /// <param name="ProjectNo"></param>
        /// <param name="PlanId"></param>
        public void InsertBudgetSourceGByPWSSD(string ProjectNo, int PlanId)
        {
            string sql = $@"INSERT INTO PROJECT_BUDGET_SOURCE_G 
                               (PROJECT_NO, 
                                BUDGET_TYPE, 
                                PLAN_YEAR, 
                                SOURCE_KIND, 
                                PLAN_ITEM_C, 
                                PLAN_SUBITEM_C, 
                                BUDGET_CENTRAL, 
                                BUDGET_LOCAL, 
                                END_CENTRAL_BALANCE, 
                                END_LOCAL_BALANCE,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
	                        SELECT
		                        @ProjectNo AS PROJECT_NO,
		                        BUDGET_TYPE,
		                        PLAN_YEAR,
		                        SOURCE_KIND,
		                        PLAN_ITEM_C,
		                        PLAN_SUBITEM_C,
		                        BUDGET_CENTRAL,
		                        BUDGET_LOCAL,
		                        END_CENTRAL_BALANCE,
		                        END_LOCAL_BALANCE,
                                @CRT_USER as CRT_DATE,
                                {DTNow} as CRT_DATE,
                                @MDF_USER as MDF_USER,
                                {DTNow} as MDF_DATE
	                        FROM {RIS_M}.PROJECT_BUDGET_SOURCE_G
	                        WHERE PROJECT_NO = @PlanId";
            ExecuteCommand(sql, new { 
                ProjectNo,
                PlanId,
                CRT_USER = UserId,
                MDF_USER = UserId});
        }

        #endregion

        #region 基本資料匯入
        /// <summary>
        /// 取得縣市代碼
        /// </summary>
        /// <param name="cityName"></param>
        /// <returns></returns>
        public string GetCityGovId (string cityName)
        {
            string sql = @"select CITY_GOV_ID 
                           from  CODE_CITY (nolock)　
                           where CITY_NAME = @cityName";
            return ExecuteQueryFirstOrDefault<string>(sql, new { cityName },MainDBKey)??"";
        }

        /// <summary>
        /// 基本資料匯入 - 新增計畫基本資料
        /// </summary>
        /// <param name="models"></param>
        public void InsertProjectBasicByExcel(ImportGeneralProjectModel model)
        {
            string sql = $@"Insert Into PROJECT_BASIC
                               (PROJECT_NO,
                                ALL_JOB,
                                PROJECT_YEAR,
                                PROJECT_NAME,
                                IS_NEW_CHECK,
                                X_COORD,
                                Y_COORD,
                                PROJECT_CATEGORY,
                                CATEGORY_ORGAN,
                                MASTER_ORGAN_C,
                                EXEC_ORGAN_C,
                                CREATEDTIME,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE) 
	                       Values ( 
                                @PROJECT_NO,
                                @ALL_JOB,
                                @PlanYear,
                                @PROJECT_NAME,
                                '0',
                                '0',
                                '0',
                                'G00',
                                '380000000A',
                                @MASTER_ORGAN_C,
                                @EXEC_ORGAN_C,
                                {DTNow},
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 基本資料匯入 - 新增計畫經費來源
        /// </summary>
        /// <param name="models"></param>
        public void InsertProjecBudgetSourceGByExcel(ImportGeneralProjectModel model)
        {
            string sql = $@"Insert into PROJECT_BUDGET_SOURCE_G 
                               (PROJECT_NO, 
                                BUDGET_CLASS,
                                BUDGET_TYPE, 
                                PLAN_YEAR,
                                SOURCE_KIND, 
                                PLAN_ITEM_C, 
                                PLAN_SUBITEM_C, 
                                BUDGET_PLAN_TYPE, 
                                BUDGET_CENTRAL, 
                                BUDGET_LOCAL, 
                                EXCEPT_PROJECT_BUDGET,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE) 
	                       Values(
                                @PROJECT_NO,
                                '',
                                'A',
                                @PlanYear,
                                '253', -- 待確認來源
                                '', 
                                '',
                                '', 
                                @BUDGET_CENTRAL,
                                @BUDGET_LOCAL, 
                                'N',
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";

            ExecuteCommand(sql, model);
        }
        #endregion
    }
}
