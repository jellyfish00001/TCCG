using Microsoft.AspNetCore.Http;
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
    public class DashBoardDac : Dac, IDashBoardDac
    {
        private readonly IUserProfile profile;

        public DashBoardDac(IConnectionControlCenter connectionControlCenter,
           IHttpContextAccessor httpContextAccessor,
           ISqlTrace trace,
           IUserProfile profile,
           IParameterAdaptor ParameterAdaptor,
           IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
            this.profile = profile;
        }



        /// <summary>
        /// 檢查該月基本資料明細檔是否存在
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public bool CheckDABProjectDataExisted(string year, string month)
        {
            string sql = @"select COUNT(*) 
                           from DABPROJECTDATA (nolock) 
                           where DAB_YEAR_YYY = @year 
                               and DAB_MONTH = @month ";
            return ExecuteQuery<int>(sql, new { year, month }).FirstOrDefault() > 0;
        }

        /// <summary>
        /// 刪除該月基本資料明細檔
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public async Task DeleteDABProjectDataByMonth(string year, string month)
        {
            string sql = @"delete DABPROJECTDATA 
                           where DAB_YEAR_YYY = @year 
                               and DAB_MONTH = @month";
            await ExecuteCommandAsync(sql, new { year, month });
        }

        /// <summary>
        /// 刪除每月綜合排序資料
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        public async Task DeleteDABCompositeByMonth(string year, string month)
        {
            string sql = @"delete DABCOMPOSITE
                           where DAB_YEAR_YYY = @year
                               and DAB_MONTH = @month";
            await ExecuteCommandAsync(sql, new { year, month });
        }

        /// <summary>
        /// 刪除所有歷年列管情形
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public async Task DeleteDABPastYears()
        {
            string sql = @"delete DABPASTYEARS";
            await ExecuteCommandAsync(sql,null);
        }

        /// <summary>
        /// 刪除機關連續落後比率
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public async Task DeleteDABDelayMonth(string year,string month)
        {
            string sql = @"delete DABDELAYMONTH where DAB_YEAR_YYY = @year and DAB_MONTH = @month";
            await ExecuteCommandAsync(sql, new { year, month });
        }

        /// <summary>
        /// 刪除行政區每月案件統計
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public async Task DeleteDABTown (string year, string month)
        {
            string sql = @"delete DABTOWN where DAB_YEAR_YYY = @year and DAB_MONTH = @month";
            await ExecuteCommandAsync(sql, new { year, month });
        }

        /// <summary>
        /// 取得當期基本資料明細檔
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public async Task<List<DABProjectDataModel>> GetDABProjectData(string year, string month)
        {
            string sql = $@"SELECT
                                @DAB_YEAR_YYYY as DAB_YEAR_YYYY,
                                @DAB_YEAR_YYY as DAB_YEAR_YYY,
                                @DAB_MONTH as DAB_MONTH,
                                M1.PROJECT_NO AS PROJECT_NO,
                                M1.PROJECT_NAME AS PROJECT_NAME,
                                M1.PROJECT_YEAR AS PROJECT_YEAR,
                                (CASE WHEN M1.FINISH_DATE > DATEADD(day,1,@YEAR_MONTH_END) THEN '4' ELSE M1.PROJECT_STATUS END) PROJECT_STATUS,
                                dbo.FN_GET_PROJ_STAGE(M1.PROJECT_NO) as PROJECT_CATEGORY,
                                M1.EXEC_ORGAN_C,
                                dbo.FN_GetOuName(M1.EXEC_ORGAN_C, 3) AS EXEC_DEPT,
                                dbo.FN_GET_DELAY_TYPE(M1.PROJECT_NO, '', @YEAR_MONTH_END) AS DELAY_TYPE,
                                M2.BUDGET_TOTAL,
                                M3.OU_SORT_ORDER AS EXEC_SORT_ORDER,
                                M1.MASTER_ORGAN_C,
                                dbo.FN_GetOuName(M1.MASTER_ORGAN_C, 3) AS MASTER_DEPT,
                                M1.TOWN_C,
                                M1.TOWN_M,
                                D1.TOWNNAME,
                                M1.CP_KIND,
                                D2.BUILD_KIND,
                                D3.SET_VALUE AS BUILD_KIND_DESC,
                                D4.BUDGET_CENTRAL,
                                dbo.FN_GET_DELAY_DAYS(M1.PROJECT_NO,@DAB_YEAR_YYY,@DAB_MONTH, @YEAR_MONTH_END) as CHKPT_DELAY_DAYS,
                                ISNULL(D5.IPC_RES_PRG - D5.IPC_ACT_PRG,0) AS DELAY_PRG
                            FROM PROJECT_BASIC (NOLOCK) M1
                            LEFT JOIN (
                                SELECT PROJECT_NO ,SUM(BUDGET_CENTRAL + BUDGET_LOCAL) AS BUDGET_TOTAL
                                FROM PROJECT_BUDGET_SOURCE_G (NOLOCK)
                                GROUP BY PROJECT_NO
                            ) M2 ON M1.PROJECT_NO = M2.PROJECT_NO
                            INNER JOIN {SC30_M}.SCORG_UNITM M3 ON M1.EXEC_ORGAN_C = M3.OU_ID
                            LEFT JOIN CODE_TOWN D1 (nolock)
	                            ON M1.TOWN_C = D1.TOWN_ID
                            LEFT JOIN PROJECT_BUILD_KIND D2 (nolock)
	                            on M1.PROJECT_NO = D2.PROJECT_NO and D2.BUILD_KIND_TYPE = '01'
                            LEFT JOIN SET_PARAM D3 (nolock)
	                            on D2.BUILD_KIND = D3.SET_TYPE and D3.SET_ITEM = 'COM_PLANKIND'
                            LEFT JOIN (
                                SELECT PROJECT_NO, SUM (BUDGET_CENTRAL) AS BUDGET_CENTRAL
                                FROM PROJECT_BUDGET_SOURCE_G (NOLOCK)
                                GROUP BY PROJECT_NO
                            ) D4 ON M1.PROJECT_NO = D4.PROJECT_NO
                            LEFT JOIN PROJECT_ENGINEERING_PROGRESS D5
	                            ON M1.PROJECT_NO = D5.PROJECT_NO 
	                            AND D5.YEAR  = @DAB_YEAR_YYY
	                            AND D5.MONTH = @DAB_MONTH
                            WHERE M1.EXEC_ORGAN_C IS NOT NULL 
                                AND M1.IS_CANCELED = 0
	                            AND M1.CREATEDTIME < DATEADD(day,1,@YEAR_MONTH_END)
                            AND (M1.PROJECT_YEAR = @PROJECT_YEAR 
	                                    OR (M1.PROJECT_YEAR < @PROJECT_YEAR AND M1.PROJECT_STATUS IN ('1', '2', '3', '4', '5', '6'))
                                        OR (year(M1.FINISH_DATE) - 1911 >= @PROJECT_YEAR AND M1.PROJECT_STATUS IN ('7', '8'))
                                )";
            int dabYearYYYY = int.Parse(year) + 1911;
            return (await ExecuteQueryAsync<DABProjectDataModel>(sql, new
            {
                PROJECT_YEAR = year,
                DAB_YEAR_YYYY = dabYearYYYY,
                DAB_YEAR_YYY = year,
                DAB_MONTH = month,
                YEAR_MONTH_START = new DateTime(dabYearYYYY, int.Parse(month), 1),
                YEAR_MONTH_END = new DateTime(dabYearYYYY, int.Parse(month), 1).AddMonths(1).AddDays(-1)
            })).ToList();
        } 

        /// <summary>
        /// 新增基本資料明細檔
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public async Task InsertDABProjectData(List<DABProjectDataModel> data)
        {
            string sql = $@"INSERT INTO DABPROJECTDATA 
                               (DAB_YEAR_YYYY,
                                DAB_YEAR_YYY,
                                DAB_MONTH,
                                PROJECT_NO, 
                                PROJECT_NAME, 
                                PROJECT_YEAR,
                                PROJECT_STATUS,
                                PROJECT_CATEGORY,
                                EXEC_ORGAN_C,
                                EXEC_DEPT,
                                DELAY_TYPE,
                                BUDGET_TOTAL,
                                EXEC_SORT_ORDER,
                                MASTER_ORGAN_C,
                                MASTER_DEPT,
                                TOWN_C,
                                TOWNNAME,
                                CP_KIND,
                                BUILD_KIND,
                                BUILD_KIND_DESC,
                                DELAY_PRG,
                                CHKPT_DELAY_DAYS,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            VALUES(
                                @DAB_YEAR_YYYY,
                                @DAB_YEAR_YYY,
                                @DAB_MONTH,
                                @PROJECT_NO,
                                @PROJECT_NAME,
                                @PROJECT_YEAR,
                                @PROJECT_STATUS,
                                @PROJECT_CATEGORY,
                                @EXEC_ORGAN_C,
                                @EXEC_DEPT,
                                @DELAY_TYPE,
                                @BUDGET_TOTAL,
                                @EXEC_SORT_ORDER,
                                @MASTER_ORGAN_C,
                                @MASTER_DEPT,
                                @TOWN_C,
                                @TOWNNAME,
                                @CP_KIND,
                                @BUILD_KIND,
                                @BUILD_KIND_DESC,
                                @DELAY_PRG,
                                @CHKPT_DELAY_DAYS,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";


            await ExecuteCommandAsync(sql, data);
        }

        /// <summary>
        /// 新增每月綜合排序資料
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public async Task InsertDABComposite(List<DABCompositeModel> data)
        {
            string sql = $@"INSERT INTO DABCOMPOSITE 
                            (
                                DAB_YEAR_YYYY,
                                DAB_YEAR_YYY,
                                DAB_MONTH,
                                DAB_KIND,
                                SET_TYPE,
                                SET_VALUE,
                                TOTAL_NUM,
                                BUDGET_TOTAL,
                                CLOSE_NUM,
                                CONFORM_NUM,
                                DELAY_NUM,
                                F1,
                                DELAY_RATE,
                                F2,
                                DELAY_NUM_D1,
                                DELAY_NUM_D2,
                                DELAY_NUM_D3,
                                CANCEL_NUM,
                                COMPLETE_RATE,
                                DELAY_F1F2,
                                DELAY_RANKING,
                                AFFECTED_SUBSIDY_NUM,
                                IS_IN_PROGRESS_DATA,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE
                                )
                            VALUES(
                                @DAB_YEAR_YYYY,
                                @DAB_YEAR_YYY,
                                @DAB_MONTH,
                                @DAB_KIND,
                                @SET_TYPE,
                                @SET_VALUE,
                                @TOTAL_NUM,
                                @BUDGET_TOTAL,
                                @CLOSE_NUM,
                                @CONFORM_NUM,
                                @DELAY_NUM,
                                @F1,
                                @DELAY_RATE,
                                @F2,
                                @DELAY_NUM_D1,
                                @DELAY_NUM_D2,
                                @DELAY_NUM_D3,
                                @CANCEL_NUM,
                                @COMPLETE_RATE,
                                @DELAY_F1F2,
                                @DELAY_RANKING,
                                @AFFECTED_SUBSIDY_NUM,
                                @IS_IN_PROGRESS_DATA,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";

            await ExecuteCommandAsync(sql, data);
        }

        /// <summary>
        /// 取得歷年列管情形資料(不含當年)
        /// </summary>
        /// <returns></returns>
        public async Task<List<DABPastYearsModel>> GetDABPastYearsData()
        {
            string sql = @" SELECT 
                                M1.DAB_YEAR_YYY + 1911 as DAB_YEAR_YYYY,
                                M1.DAB_YEAR_YYY,
                                M1.EXEC_ORGAN_C,
                                M1.TOTAL_NUM,
                                M1.TOTAL_EXS,
                                D1.DELAY_RATE
                            FROM (
                                SELECT MM1.PROJECT_YEAR  as  DAB_YEAR_YYY, 
    	                               MM1.EXEC_ORGAN_C,
                                COUNT( MM1.PROJECT_YEAR) as TOTAL_NUM,
                                ISNULL(SUM(BUDGET_TOTAL),0) as TOTAL_EXS
                                FROM (
                                    SELECT M1.PROJECT_NO,
                                            M1.PROJECT_NAME,
                                            M1.PROJECT_YEAR,
                                            M1.EXEC_ORGAN_C,
                                            dbo.FN_GetOuName(M1.EXEC_ORGAN_C, 3) AS EXEC_DEPT,
                                            M2.BUDGET_TOTAL
                                    FROM PROJECT_BASIC (NOLOCK) M1
                                    LEFT JOIN (
                                    SELECT PROJECT_NO ,SUM(BUDGET_CENTRAL + BUDGET_LOCAL) AS BUDGET_TOTAL
                                    FROM PROJECT_BUDGET_SOURCE_G (NOLOCK)
                                    GROUP BY PROJECT_NO
                                    ) M2 ON M1.PROJECT_NO = M2.PROJECT_NO

                                    WHERE M1.EXEC_ORGAN_C IS NOT NULL 
                                    AND M1.IS_CANCELED = 0
                                ) MM1

                                GROUP BY  MM1.PROJECT_YEAR,MM1.EXEC_ORGAN_C
                            ) M1 --立案件數、經費資料
                            LEFT JOIN (
                                select M.DAB_YEAR_YYY, 
    	                               M.EXEC_ORGAN_C,
                                       ROUND(SUM(M.DELAY_RATE)/12,2) as DELAY_RATE
                                from (
                                    select  DAB_YEAR_YYY , DAB_MONTH,
        	                            SET_TYPE as EXEC_ORGAN_C,
                                        DELAY_RATE
                                    from DABCOMPOSITE D (NOLOCK)
                                    where  DAB_KIND = 'A'
                                    ) M
                                group by M.DAB_YEAR_YYY,EXEC_ORGAN_C
                            ) D1 --落後比率資料
                                on M1.DAB_YEAR_YYY = D1.DAB_YEAR_YYY
    	                            and M1.EXEC_ORGAN_C = D1.EXEC_ORGAN_C
                            WHERE M1.DAB_YEAR_YYY >= 108";

            return (await ExecuteQueryAsync<DABPastYearsModel>(sql)).ToList();
        }

        /// <summary>
        /// 取得年度(不含當月)執行機關平均落後比率
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public async Task<List<ExecOrgDelayModel>> GetExecAvgDelayRateOfYear(string year, string month)
        {
            string sql = @" select SET_TYPE as EXEC_ORGAN_C,
                                   AVG(DELAY_RATE) as DELAY_RATE
                            from DABCOMPOSITE D (NOLOCK)
                            where DAB_YEAR_YYY = @year
                                and DAB_KIND = 'A' 
                                and DAB_MONTH != @month
                            group by SET_TYPE";
            return (await ExecuteQueryAsync<ExecOrgDelayModel>(sql,new { year, month })).ToList();
        }

        /// <summary>
        /// 新增歷年列管情形資料
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public async Task InsertDABPastYears(List<DABPastYearsModel> models)
        {
            string sql = @"INSERT INTO DABPASTYEARS(
                                DAB_YEAR_YYYY,
                                DAB_YEAR_YYY,
                                EXEC_ORGAN_C,
                                TOTAL_NUM,
                                TOTAL_EXS,
                                DELAY_RATE)
                            VALUES(
                                @DAB_YEAR_YYYY,
                                @DAB_YEAR_YYY,
                                @EXEC_ORGAN_C,
                                @TOTAL_NUM,
                                @TOTAL_EXS,
                                @DELAY_RATE)";
            await ExecuteCommandAsync(sql, models);
        }

        /// <summary>
        /// 新增機關連續落後比率
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public async Task InsertDABDelayMonth(List<DABDelayMonthModel> models)
        {
            string sql = @"INSERT INTO DABDELAYMONTH(
                                DAB_YEAR_YYYY,
                                DAB_YEAR_YYY,
                                DAB_MONTH,
                                EXEC_ORGAN_C,
                                EXEC_DEPT,
                                TOTAL_NUM,
                                DELAY_NUM,
                                DELAY_RATE,
                                DELAY_NUM_3,
                                DELAY_NUM_3_RATE,
                                DELAY_NUM_2,
                                DELAY_NUM_2_RATE,
                                DELAY_NUM_1,
                                DELAY_NUM_1_RATE,
                                IS_IN_PROGRESS_DATA)
                            VALUES(
                                @DAB_YEAR_YYYY,
                                @DAB_YEAR_YYY,
                                @DAB_MONTH,
                                @EXEC_ORGAN_C,
                                @EXEC_DEPT,
                                @TOTAL_NUM,
                                @DELAY_NUM,
                                @DELAY_RATE,
                                @DELAY_NUM_3,
                                @DELAY_NUM_3_RATE,
                                @DELAY_NUM_2,
                                @DELAY_NUM_2_RATE,
                                @DELAY_NUM_1,
                                @DELAY_NUM_1_RATE,
                                @IS_IN_PROGRESS_DATA)";
            await ExecuteCommandAsync(sql, models);
        }

        /// <summary>
        /// 新增行政區每月案件統計
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public async Task InsertDABTownData(List<DABTownModel> models)
        {
            string sql = @"INSERT INTO DABTOWN(
                                DAB_YEAR_YYYY,
                                DAB_YEAR_YYY,
                                DAB_MONTH,
                                EXEC_ORGAN_C,
                                TOWN,
                                TOWN_NAME,
                                TOTAL_NUM,
                                STAGE_E2_NUM,
                                STAGE_E2_DELAY_NUM,
                                STAGE_E1_NUM,
                                STAGE_E1_DELAY_NUM,
                                STAGE_E3_NUM,
                                STAGE_E3_DELAY_NUM)
                            VALUES(
                                @DAB_YEAR_YYYY,
                                @DAB_YEAR_YYY,
                                @DAB_MONTH,
                                @EXEC_ORGAN_C,
                                @TOWN,
                                @TOWN_NAME,
                                @TOTAL_NUM,
                                @STAGE_E2_NUM,
                                @STAGE_E2_DELAY_NUM,
                                @STAGE_E1_NUM,
                                @STAGE_E1_DELAY_NUM,
                                @STAGE_E3_NUM,
                                @STAGE_E3_DELAY_NUM)";
            await ExecuteCommandAsync(sql, models);
        }

        /// <summary>
        /// 取得統計項目的列管件數、總經費、落後比率資料
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        public async Task<List<DABCompositeModel>> GetDABComposite(DABCompositeQueryModel queryModel)
        {
            StringBuilder sql = new StringBuilder(@"
                select 
                    DAB_YEAR_YYYY,
                    DAB_YEAR_YYY,
                    DAB_MONTH,
                    SET_TYPE,
                    SET_VALUE,
                    DAB_KIND,
                    TOTAL_NUM ,
                    BUDGET_TOTAL, 
                    DELAY_RATE, 
                    DELAY_NUM ,
                    F1,
                    F2,
                    DELAY_F1F2,
                    AFFECTED_SUBSIDY_NUM
                from DABCOMPOSITE D (nolock)
                where  CAST(DAB_YEAR_YYYY+DAB_MONTH+'01' as DATE) >= @QueryPeriodSt
                    and CAST(DAB_YEAR_YYYY+DAB_MONTH+'01' as DATE) <= @QueryPeriodEnd 
                    and IS_IN_PROGRESS_DATA = @IS_IN_PROGRESS_DATA ");
            // 儀表板類別
            if (queryModel.DabKinds.Any())
            {
                sql.Append(" and DAB_KIND in @DabKinds");
            }
            // 執行機關、行政區、建設類別
            if (!string.IsNullOrEmpty(queryModel.SET_TYPE))
            {
                sql.Append(" and SET_TYPE = @SET_TYPE");
            }
            return (await ExecuteQueryAsync<DABCompositeModel>(sql.ToString(),queryModel)).ToList();
        }

        /// <summary>
        /// 取得計畫落後資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<DABProjectDataModel>> GetProjectDelayType (DABCompositeQueryModel model)
        {
            string sql = @"select 
                                PROJECT_NO , -- 計畫編號
                                PROJECT_CATEGORY , --計畫類別(規畫中、施工、驗收)
                                DELAY_TYPE --落後類型 
                            from DABPROJECTDATA D (nolock)
                            where CAST(DAB_YEAR_YYYY+DAB_MONTH+'01' as DATE) >= @QueryPeriodSt
                                and CAST(DAB_YEAR_YYYY+DAB_MONTH+'01' as DATE) <= @QueryPeriodEnd 
                                and DELAY_TYPE is not null and DELAY_TYPE != ''";
            // 執行機關
            if (!string.IsNullOrEmpty(model.SET_TYPE))
            {
                sql += @" and D.EXEC_ORGAN_C = @SET_TYPE";
            }
            return (await ExecuteQueryAsync<DABProjectDataModel>(sql, model)).ToList();
        }

        /// <summary>
        /// 取得全府平均落後比率
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        public async Task<List<DABCompositeModel>> GetAnnualOrgAvgDelayRate(DABCompositeQueryModel queryModel)
        {
            string sql = @" select 
                                DAB_YEAR_YYYY,
                                DAB_YEAR_YYY,
                                DAB_MONTH ,
                                AVG(DELAY_RATE) as DELAY_RATE
                            from DABCOMPOSITE D (nolock)
                            where CAST(DAB_YEAR_YYYY+DAB_MONTH+'01' as DATE) >= @QueryPeriodSt
	                            and CAST(DAB_YEAR_YYYY+DAB_MONTH+'01' as DATE) <= @QueryPeriodEnd
	                            and DAB_KIND = 'A'
                            group by DAB_YEAR_YYY, DAB_MONTH";
            return (await ExecuteQueryAsync<DABCompositeModel>(sql, queryModel)).ToList();
        }

        /// <summary>
        /// 取得機關連續3個月落後比率
        /// </summary>
        /// <param name="QueryPeriodSt"></param>
        /// <param name="QueryPeriodEnd"></param>
        /// <param name="EXEC_ORGAN_C"></param>
        /// <returns></returns>
        public async Task<List<DABDelayMonthModel>> GetOrgRecentDelayRates(DateTime QueryPeriodSt, DateTime QueryPeriodEnd, 
            string EXEC_ORGAN_C, bool IS_IN_PROGRESS_DATA)
        {
            string sql = @" select 
                                DAB_YEAR_YYYY,
                                DAB_YEAR_YYY,
                                DAB_MONTH ,
                                DELAY_NUM_3_RATE
                            from DABDELAYMONTH D (nolock)
                                where CAST(DAB_YEAR_YYYY+DAB_MONTH+'01' as DATE) >= @QueryPeriodSt
                                and CAST(DAB_YEAR_YYYY+DAB_MONTH+'01' as DATE) <= @QueryPeriodEnd 
                                and IS_IN_PROGRESS_DATA = @IS_IN_PROGRESS_DATA";
            if (!string.IsNullOrEmpty(EXEC_ORGAN_C))
            {
                sql += " and EXEC_ORGAN_C = @EXEC_ORGAN_C";
            }
            return (await ExecuteQueryAsync<DABDelayMonthModel>(sql, new { QueryPeriodSt, QueryPeriodEnd, EXEC_ORGAN_C, IS_IN_PROGRESS_DATA })).ToList();
        }

        /// <summary>
        /// 取得年度統計資料
        /// </summary>
        /// <param name="years"></param>
        /// <returns></returns>
        public async Task<List<DABPastYearsModel>>GetDABPastYears(List<string> years)
        {
            string sql = @"select DAB_YEAR_YYY,
                                  TOTAL_NUM,
                                  TOTAL_EXS,
                                  DELAY_RATE 
                            from DABPASTYEARS D (nolock)
                            where DAB_YEAR_YYY in @years";
            return (await ExecuteQueryAsync<DABPastYearsModel>(sql,new { years })).ToList();
        }

        /// <summary>
        /// 重要儀表板 - 取得建設類別、行政區 件數及經費資料
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        public async Task<List<ProjectTotalDataModel>> GetProjectTotalData(ProjectTotalQueryModel queryModel)
        {
            string sql = @"select TOTAL_NUM,
                                  BUDGET_TOTAL, 
                                  DAB_KIND,
                                  SET_TYPE
                           from DABCOMPOSITE D3 (nolock)
                           where DAB_KIND in ('B','C')
                               and DAB_YEAR_YYY = @DAB_YEAR_YYY 
                               and DAB_MONTH = @DAB_MONTH";
            return (await ExecuteQueryAsync<ProjectTotalDataModel>(sql, queryModel)).ToList();
        }

        /// <summary>
        /// 件數及經費執行情形 - 頁面資料
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        public async Task<List<ProjectTotalDataModel>> GetDashBoardCountAndBudget(ProjectTotalQueryModel queryModel)
        {
            string dabKindColumn = "",valueColumn = "";
            string condition = " ";
            switch (queryModel.DAB_KIND) {
                case "A":
                    dabKindColumn= "EXEC_ORGAN_C";
                    valueColumn = "EXEC_DEPT";
                    break;
                case "B":
                    dabKindColumn = "TOWN_C";
                    valueColumn = "TOWNNAME";
                    break;
                case "C":
                    dabKindColumn = "BUILD_KIND"; 
                    valueColumn = "BUILD_KIND_DESC";
                    break;
            }

            if(queryModel.ProjectStatusType == "2")
            {
                condition = " and PROJECT_STATUS = '4' ";
            }
            
            string sql = $@"select count(PROJECT_NO) as TOTAL_NUM,
                                   sum(BUDGET_TOTAL) as BUDGET_TOTAL,
                                   {dabKindColumn} as SET_TYPE,
                                   {valueColumn} as SET_VALUE
                            from DABPROJECTDATA (nolock)
                            where 1 = 1
                            {condition}
                            and {dabKindColumn} is not null
                            group by {dabKindColumn},{valueColumn}";

            return (await ExecuteQueryAsync<ProjectTotalDataModel>(sql, queryModel)).ToList() ;
        }

        /// <summary>
        /// 取得歷年列管情形資料
        /// </summary>
        /// <param name="EXEC_ORGAN_C"></param>
        /// <returns></returns>
        public async Task<List<DABPastYearsModel>> GetPastYearsData(string EXEC_ORGAN_C)
        {
            string sql = @" select 
	                            DAB_YEAR_YYYY ,
	                            DAB_YEAR_YYY ,
	                            EXEC_ORGAN_C ,
	                            dbo.FN_GetOuName(EXEC_ORGAN_C,3) as EXEC_DEPT ,
	                            TOTAL_NUM ,
	                            TOTAL_EXS ,
	                            DELAY_RATE 
                            from DABPASTYEARS D (nolock)";

            if(!string.IsNullOrEmpty(EXEC_ORGAN_C))
            {
                sql += " where D.EXEC_ORGAN_C = @EXEC_ORGAN_C";
            }

            return (await ExecuteQueryAsync<DABPastYearsModel>(sql, new { EXEC_ORGAN_C })).ToList();
        }

        /// <summary>
        /// 取得重大工程進度資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<DABTownModel> GetDashBoardEngProgress(DashBoardQueryModel model)
        {
            string sql = @"select 
	                            SUM(STAGE_E2_NUM) as STAGE_E2_NUM ,
	                            SUM(STAGE_E2_DELAY_NUM) as STAGE_E2_DELAY_NUM ,
	                            SUM(STAGE_E1_NUM) as STAGE_E1_NUM ,
	                            SUM(STAGE_E1_DELAY_NUM) as STAGE_E1_DELAY_NUM ,
	                            SUM(STAGE_E3_NUM) as STAGE_E3_NUM,
	                            SUM(STAGE_E3_DELAY_NUM) as STAGE_E3_DELAY_NUM  
                            from DABTOWN (nolock)
                            where DAB_YEAR_YYY = @DAB_YEAR_YYY
	                            and DAB_MONTH  = @DAB_MONTH";

            if (!string.IsNullOrEmpty(model.TOWN_C))
            {
                sql += " and TOWN = @TOWN_C";
            }

            if (!string.IsNullOrEmpty(model.EXEC_ORGAN_C))
            {
                sql += " and EXEC_ORGAN_C = @EXEC_ORGAN_C";
            }

            return await ExecuteQueryFirstOrDefaultAsync<DABTownModel>(sql, model);
        }

        /// <summary>
        /// 取得近兩年預計完工計畫資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ProjectFinishModel>> GetProjectFinishData(DashBoardQueryModel model)
        {
            string sql = @" SELECT T1.PROJECT_NO, 
	                               T1.ESTIMATED_ENDDATE,
	                               D1.FINISH_DATE
                            FROM (
	                             SELECT *, ROW_NUMBER() OVER (PARTITION BY
	                             PROJECT_NO ORDER BY ESTIMATED_ENDDATE desc) AS row_number
	                             FROM PROJECT_CHECKITEM (nolock)
                            ) AS T1
                            INNER JOIN PROJECT_BASIC D1 (nolock)
	                            ON T1.PROJECT_NO = D1.PROJECT_NO
	                            AND D1.IS_CANCELED = 0 
                            WHERE T1.row_number = 1
	                            AND ESTIMATED_ENDDATE is not null 
	                            -- 近兩年
	                            AND YEAR(ESTIMATED_ENDDATE) >= (YEAR(GETDATE())- 1) 
	                            AND YEAR(ESTIMATED_ENDDATE) <= (YEAR(GETDATE()))";

            if(!string.IsNullOrEmpty(model.TOWN_C))
            {
                sql += @" AND D1.TOWN_C = @TOWN_C";
            }

            if (!string.IsNullOrEmpty(model.EXEC_ORGAN_C))
            {
                sql += @" AND D1.EXEC_ORGAN_C = @EXEC_ORGAN_C";
            }

            return (await ExecuteQueryAsync<ProjectFinishModel>(sql,model)).ToList();
        }
        
        /// <summary>
        /// 取得行政區落後情形統計資料
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        public async Task<List<TownDelayModel>> GetTownDelayData(DashBoardQueryModel queryModel)
        {
            string condition = "";
            if (!string.IsNullOrEmpty(queryModel.EXEC_ORGAN_C))
                condition = @" and EXEC_ORGAN_C  = @EXEC_ORGAN_C ";

            if (queryModel.IS_IN_PROGRESS_DATA)
            {
                condition += @" and D.PROJECT_STATUS in ('4','5','6')";
            }
            else
            {
                condition += @" and D.PROJECT_STATUS in ('4','5','6','7','8')";
            }

            string sql = $@" select M1.TOWN_ID as TOWN_C,
		                            M1.TOWNNAME ,
		                            D1.EngDelayBelow10Cnt,
		                            D1.EngDelayBelow20Cnt,
		                            D1.EngDelayOver20Cnt,
		                            D1.ChkPtDelayBelow3MonthsCnt,
		                            D1.ChkPtDelayBelow6MonthsCnt,
		                            D1.ChkPtDelayOver6MonthsCnt
                            from CODE_TOWN M1 (nolock)
                            left join (
	                            select  TOWN_C , 
	                                SUM(CASE WHEN D.DELAY_PRG < 10 THEN 1 ELSE 0 END) as EngDelayBelow10Cnt, 
	                                SUM(CASE WHEN D.DELAY_PRG > 10 and D.DELAY_PRG < 20 THEN 1 ELSE 0 END)as EngDelayBelow20Cnt,
	                                SUM(CASE WHEN D.DELAY_PRG > 20 THEN 1 ELSE 0 END) as EngDelayOver20Cnt,
	                                SUM(CASE WHEN D.CHKPT_DELAY_DAYS < 90 THEN 1 ELSE 0 END) as ChkPtDelayBelow3MonthsCnt,
	                                SUM(CASE WHEN D.CHKPT_DELAY_DAYS > 90 and D.CHKPT_DELAY_DAYS < 180 THEN 1 ELSE 0 END) as ChkPtDelayBelow6MonthsCnt,
	                                SUM(CASE WHEN D.CHKPT_DELAY_DAYS > 180 THEN 1 ELSE 0 END) as ChkPtDelayOver6MonthsCnt
	                            from DABPROJECTDATA D (nolock)
	                            where DAB_YEAR_YYY  = @DAB_YEAR_YYY
		                            and DAB_MONTH = @DAB_MONTH
		                            {condition}
	                            group by TOWN_C
                            ) D1 on M1.TOWN_ID = D1.TOWN_C
                            where M1.TOWN_ID != 'H00' 
                                and M1.TOWN_ID != 'H01'";

            return (await ExecuteQueryAsync<TownDelayModel>(sql, queryModel)).ToList();
        }
    }        
}
