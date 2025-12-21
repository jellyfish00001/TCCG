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
    public class SuperiorDac : Dac, ISuperiorDac
    {
        private readonly IUserProfile profile;

        public SuperiorDac(IConnectionControlCenter connectionControlCenter,
           IHttpContextAccessor httpContextAccessor,
           ISqlTrace trace,
           IUserProfile profile,
           IParameterAdaptor ParameterAdaptor,
           IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
            this.profile = profile;
        }

        /// <summary>
        /// 設定決策分析SQL
        /// </summary>
        /// <param name="columns">欄位清單</param>
        /// <returns></returns>
        private string SetDecisionSql(List<string> columns)
        {
            return $@"
                select 
                    {string.Join(", ", columns.Where(x => !string.IsNullOrEmpty(x)))}
                from PROJECT_BASIC (nolock) M1
                left join PROJECT_BUILD_KIND (nolock) D1 on D1.PROJECT_NO = M1.PROJECT_NO and D1.BUILD_KIND_TYPE = '01'
                left join
                (
                    select
                        row_number() over (partition by ep.PROJECT_NO order by ep.YEAR desc, ep.MONTH desc) as SORT,
		                ep.PROJECT_NO,
						--實際施工進度-預定施工進度 落後施工進度落後判斷
						ISNULL(ep.IPC_RES_PRG-ep.IPC_ACT_PRG,0) AS ENG_PRG_OFFSET ,
		                -- 實際進度-預定進度
						ISNULL([dbo].[FN_GET_PROGRESS] (wp.PROJECT_NO,wp.CP_KIND,GETDATE()),0)  as PRG_OFFSET,
						ep.IS_DELAY,
		                ep.MONTH as EXEC_MONTH,
		                ep.YEAR as EXEC_YEAR,
		                dc.DELAY_KIND,
                        dc.DELAY_SUBCLASS_C,
                        cdc.DELAY_CLASS_SUB_ITEM
                    from  PROJECT_ENGINEERING_PROGRESS (nolock) ep 
					left join  PROJECT_BASIC (nolock) wp on wp.PROJECT_NO = ep.PROJECT_NO
                    left join PROJECT_DELAY_CAUSAL (nolock) dc on dc.PROJECT_NO = ep.PROJECT_NO and dc.DATA_YEAR = ep.YEAR and dc.DATA_MONTH = ep.MONTH
                    left join CODE_DELAY_CLASS (nolock) cdc on dc.DELAY_SUBCLASS_C = cdc.DELAY_CLASS_SUB_ID
					where ep.YEAR =(SELECT TOP(1)[PROJECT_YEAR] FROM [dbo].[PROJECT_FILL_CYCLE]   order by [FILL_START_DATE] desc) 
						and ep.MONTH =(SELECT TOP(1) [PROJECT_MONTH]  FROM [dbo].[PROJECT_FILL_CYCLE]   order by [FILL_START_DATE] desc)
                ) as D2 on D2.PROJECT_NO = M1.PROJECT_NO and D2.SORT = 1
                left join (
                    select
                        PROJECT_NO,
                        sum(BUDGET_CENTRAL +BUDGET_LOCAL) as PROJ_BUDGET
                    from PROJECT_BUDGET_SOURCE_G (nolock)
                    group by PROJECT_NO
                ) as D3 on D3.PROJECT_NO = M1.PROJECT_NO
                left join SUPERIOR_FAVORITEPROJECT (nolock) D4 on M1.PROJECT_NO = D4.PROJECT_NO and D4.CRT_USER = @CRT_USER
                left join (
                    select 
                        main.PROJECT_NO,
                        Max(CASE 
	                        WHEN item.CTRL_POINT = 'A' and ACTUAL_ENDDATE is not null then 'A'
	                        WHEN item.CTRL_POINT = 'B' and ACTUAL_ENDDATE is not null then 'B'
	                        ELSE NULL 
                        END) as ENGNEER_STAGE
                    from PROJECT_CHECKITEM (nolock) main
                    left join CODE_CHECKPOINT_ITEM (nolock) item on main.CHECKITEM_SEQ = item.SEQ
                    group by main.PROJECT_NO
                ) as D5 on D5.PROJECT_NO = M1.PROJECT_NO
                where M1.PROJECT_STATUS in (4, 5, 6, 7)
                    and M1.IS_CANCELED = 0";
        }

        #region 重大建設分析
        /// <summary>
        /// 設定重大建設查詢條件
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="model"></param>
        private void SetAnalysisFilter(StringBuilder sql, AnalysisQueryModel model)
        {
            // 案件類型
            if (!string.IsNullOrEmpty(model.CP_KIND))
            {
                sql.AppendLine("and M1.CP_KIND = @CP_KIND");
            }

            // 結案狀態
            if (!string.IsNullOrEmpty(model.IS_PROJECT_FINISH))
            {
                sql.AppendLine("and M1.IS_PROJECT_FINISH = @IS_PROJECT_FINISH");
            }

            // 計畫經費
            if (!string.IsNullOrEmpty(model.PROJ_BUDGET))
            {
                switch (model.PROJ_BUDGET)
                {
                    case "0":
                        sql.AppendLine("and 10000000 < D3.PROJ_BUDGET and D3.PROJ_BUDGET <= 30000000");
                        break;
                    case "1":
                        sql.AppendLine("and 30000000 < D3.PROJ_BUDGET and D3.PROJ_BUDGET <= 50000000");
                        break;
                    case "2":
                        sql.AppendLine("and 50000000 < D3.PROJ_BUDGET and D3.PROJ_BUDGET <= 100000000");
                        break;
                    case "3":
                        sql.AppendLine("and 100000000 < D3.PROJ_BUDGET");
                        break;
                }
            }

            // 主管機關
            if (!string.IsNullOrEmpty(model.MASTER_DEPT))
            {
                sql.AppendLine("and M1.MASTER_ORGAN_C = @MASTER_DEPT");
            }

            // 執行機關
            if (!string.IsNullOrEmpty(model.EXEC_DEPT))
            {
                sql.AppendLine("and M1.EXEC_ORGAN_C = @EXEC_DEPT");
            }

            // 主頁點擊的型態 & 主頁點擊的值
            if (!string.IsNullOrEmpty(model.MAIN_TYPE) && !string.IsNullOrEmpty(model.MAIN_VALUE))
            {
                switch (model.MAIN_TYPE)
                {
                    case "0": // 建設類別
                        sql.AppendLine("and D1.BUILD_KIND = @MAIN_VALUE");
                        break;
                    case "1": // 機關
                        sql.AppendLine("and M1.MASTER_ORGAN_C = @MAIN_VALUE");
                        break;
                    case "2": // 辦理地點
                        sql.AppendLine("and M1.TOWN_C = @MAIN_VALUE");
                        break;
                }
            }

            // 是否取得符合
            if (model.IS_GET_MATCH.HasValue)
            {
                if (model.IS_GET_MATCH.Value)
                {
                    sql.AppendLine($"and isnull(D2.PRG_OFFSET, 0) >= 0");
                }
                else
                {
                    sql.AppendLine($"and isnull(D2.PRG_OFFSET, 0) < 0");
                }
            }

            // 次頁點擊的型態 & 次頁點擊的值
            if (!string.IsNullOrEmpty(model.DETAIL_TYPE) && !string.IsNullOrEmpty(model.DETAIL_VALUE))
            {
                switch (model.DETAIL_TYPE)
                {
                    case "0": // 建設類別
                        sql.AppendLine("and D1.BUILD_KIND = @DETAIL_VALUE");
                        break;
                    case "1": // 機關
                        sql.AppendLine("and M1.MASTER_ORGAN_C = @DETAIL_VALUE");
                        break;
                    case "2": // 辦理地點
                        sql.AppendLine("and M1.TOWN_C = @DETAIL_VALUE");
                        break;
                }
            }

            // 落後類別
            if (!string.IsNullOrEmpty(model.DELAY_KIND))
            {
                sql.AppendLine("and D2.DELAY_KIND = @DELAY_KIND");
            }

            // 落後次類別代碼
            if (!string.IsNullOrEmpty(model.DELAY_SUBCLASS_C))
            {
                sql.AppendLine("and D2.DELAY_SUBCLASS_C = @DELAY_SUBCLASS_C");
            }

            // 是否只有主辦權限
            if (model.IS_HAND_ROLE)
            {
                sql.AppendLine($"and (M1.MASTER_ORGAN_C = @OrgId or M1.EXEC_ORGAN_C = @OrgId)");
            }
        }

        /// <summary>
        /// 取得圖表統計
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ChartStatModel>> GetChartStats(AnalysisQueryModel model)
        {
            // 依據圖表類別取得欄位
            string column = GetColumnName(model.CHART_TYPE);
            string columnSql = string.IsNullOrEmpty(column) ? string.Empty : $"{column} as Value";
            string groupBySql = string.IsNullOrEmpty(column) ? string.Empty : $"{column},";

            StringBuilder sql = new StringBuilder();
            sql.AppendLine(SetDecisionSql(new List<string>
            {
                columnSql,
                "isnull(D2.PRG_OFFSET, 0) as PRG_OFFSET",
                "count(distinct M1.PROJECT_NO) as Cnt",
                "sum(D3.PROJ_BUDGET) as PROJ_BUDGET"
            }));

            SetAnalysisFilter(sql, model);

            // group by
            sql.AppendLine($"group by {groupBySql} isnull(D2.PRG_OFFSET, 0)");
            model.OrgId = profile.GetLoginUser().ORG_ID;
            model.CRT_USER = UserId;
            return await ExecuteQueryAsync<ChartStatModel>(sql.ToString(), model);
        }

        /// <summary>
        /// 依據圖表類別取得欄位
        /// </summary>
        /// <param name="chartType">圖表類別</param>
        /// <returns></returns>
        private string GetColumnName(int chartType)
        {
            switch (chartType)
            {
                case 0: // 建設類別 
                    return "D1.BUILD_KIND";
                case 1: // 機關 
                    return "M1.MASTER_ORGAN_C";
                case 2: // 辦理地點
                    return "M1.TOWN_C";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 取得分析計劃清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ChartStatBehindModel>> GetChartStatBehinds(AnalysisQueryModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(SetDecisionSql(new List<string>
            {
                "M1.PROJECT_NO",
                "D2.DELAY_KIND",
                "D2.DELAY_SUBCLASS_C",
                "D2.DELAY_CLASS_SUB_ITEM"
            }));

            SetAnalysisFilter(sql, model);
            model.OrgId = profile.GetLoginUser().ORG_ID;
            model.CRT_USER = UserId;
            return await ExecuteQueryAsync<ChartStatBehindModel>(sql.ToString(), model);
        }

        /// <summary>
        /// 取得分析計劃清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<DecisionPlanModel>> GetAnalyzePlan(AnalysisQueryModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(SetDecisionSql(new List<string>
            {
                "M1.PROJECT_NO",
                "M1.PROJECT_NAME",
                "D3.PROJ_BUDGET",
                "isnull(D2.PRG_OFFSET, 0) as PRG_OFFSET",
                "dbo.FN_GetOuName(M1.MASTER_ORGAN_C, 3) as MASTER_ORGAN_NAME",
                "dbo.FN_GetOuName(M1.EXEC_ORGAN_C, 3) as EXEC_ORGAN_NAME",
                "(case when D4.PROJECT_NO is null then 0 else 1 end) as PIS_SELECT"
            }));

            SetAnalysisFilter(sql, model);

            sql.AppendLine("order by (case when D4.PROJECT_NO is null then 0 else 1 end) desc, M1.PROJECT_NO");
            model.OrgId = profile.GetLoginUser().ORG_ID;
            model.CRT_USER = UserId;
            return (await ExecuteQueryAsync<DecisionPlanModel>(sql.ToString(), model)).ToList();
        }
        #endregion 重大建設分析

        #region 建設類別查詢
        /// <summary>
        /// 取得建設類別清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ProgessModel>> GetProgesses(ProgessQueryModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(SetDecisionSql(new List<string>
            {
                "M1.PROJECT_NO",
                "M1.PROJECT_NAME",
                "M1.X_COORD",
                "M1.Y_COORD",
                "dbo.FN_GetSetParam('TOWN_ID', M1.TOWN_C) as TOWN_NAME",
                "isnull(D2.PRG_OFFSET, 0) as PRG_OFFSET",
                "D5.ENGNEER_STAGE",
                "dbo.FN_GetOuName(M1.EXEC_ORGAN_C, 3) as EXEC_ORGAN_NAME",
                "D3.PROJ_BUDGET"
            }));

            // 主管機關
            if (!string.IsNullOrEmpty(model.MASTER_DEPT))
            {
                sql.AppendLine("and M1.MASTER_ORGAN_C = @MASTER_DEPT");
            }

            // 執行機關
            if (!string.IsNullOrEmpty(model.EXEC_DEPT))
            {
                sql.AppendLine("and M1.EXEC_ORGAN_C = @EXEC_DEPT");
            }

            // 類別
            if (model.BUILD_KIND.Any())
            {
                sql.AppendLine("and D1.BUILD_KIND in @BUILD_KIND");
            }

            // 是否只有主辦權限
            if (model.IS_HAND_ROLE)
            {
                string orgId = profile.GetLoginUser().ORG_ID;
                sql.AppendLine($"and (M1.MASTER_ORGAN_C = @OrgId or M1.EXEC_ORGAN_C = @OrgId)");
            }
            model.OrgId = profile.GetLoginUser().ORG_ID;
            model.CRT_USER = UserId;
            return (await ExecuteQueryAsync<ProgessModel>(sql.ToString(), model)).ToList();
        }
        #endregion 建設類別查詢

        #region 區域統計分析
        /// <summary>
        /// 設定區域統計查詢條件
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="model"></param>
        private void SetRegionFilter(StringBuilder sql, RegionQueryModel model)
        {
            // 主管機關
            if (!string.IsNullOrEmpty(model.MASTER_DEPT))
            {
                sql.AppendLine("and M1.MASTER_ORGAN_C = @MASTER_DEPT");
            }

            // 執行機關
            if (!string.IsNullOrEmpty(model.EXEC_DEPT))
            {
                sql.AppendLine("and M1.EXEC_ORGAN_C = @EXEC_DEPT");
            }

            // 工程階段
            if (!string.IsNullOrEmpty(model.ENGNEER_STAGE))
            {
                if (model.ENGNEER_STAGE == "null")
                {
                    sql.AppendLine("and D5.ENGNEER_STAGE is null");
                }
                else
                {
                    sql.AppendLine("and D5.ENGNEER_STAGE = @ENGNEER_STAGE");
                }
            }

            // 辦理地點
            if (!string.IsNullOrEmpty(model.TOWN_C))
            {
                sql.AppendLine("and M1.TOWN_C = @TOWN_C");
            }

            // 是否只有主辦權限
            if (model.IS_HAND_ROLE)
            {
                sql.AppendLine($"and (M1.MASTER_ORGAN_C = @OrgId or M1.EXEC_ORGAN_C = @OrgId)");
            }
        }

        /// <summary>
        /// 取得區域統計清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IEnumerable<RegionModel>> GetRegions(RegionQueryModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(SetDecisionSql(new List<string>
            {
                "M1.PROJECT_NO",
                "M1.TOWN_C",
                "isnull(D2.PRG_OFFSET, 0) as PRG_OFFSET"
            }));

            SetRegionFilter(sql, model);
            model.OrgId = profile.GetLoginUser().ORG_ID;
            model.CRT_USER = UserId;
            return await ExecuteQueryAsync<RegionModel>(sql.ToString(), model);
        }

        /// <summary>
        /// 取得區域統計計劃清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<DecisionPlanModel>> GetRegionPlan(RegionQueryModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(SetDecisionSql(new List<string>
            {
                "M1.PROJECT_NO",
                "M1.PROJECT_NAME",
                "D3.PROJ_BUDGET",
                "isnull(D2.PRG_OFFSET, 0) as PRG_OFFSET",
                "dbo.FN_GetOuName(M1.MASTER_ORGAN_C, 3) as MASTER_ORGAN_NAME",
                "dbo.FN_GetOuName(M1.EXEC_ORGAN_C, 3) as EXEC_ORGAN_NAME",
                "(case when D4.PROJECT_NO is null then 0 else 1 end) as PIS_SELECT"
            }));

            SetRegionFilter(sql, model);

            sql.AppendLine("order by (case when D4.PROJECT_NO is null then 0 else 1 end) desc, M1.PROJECT_NO");
            model.OrgId = profile.GetLoginUser().ORG_ID;
            model.CRT_USER = UserId;
            return (await ExecuteQueryAsync<DecisionPlanModel>(sql.ToString(), model)).ToList();
        }
        #endregion 區域統計分析

        #region 落後案件查詢
        /// <summary>
        /// 取得區域統計清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<DelayPlanModel>> GetDelayPlans(DelayPlanQueryModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(SetDecisionSql(new List<string>
            {
                "(case when D4.PROJECT_NO is null then 0 else 1 end) as PIS_SELECT",
                "M1.PROJECT_NO",
                "M1.PROJECT_NAME",
                "dbo.FN_GetOuName(M1.MASTER_ORGAN_C, 3) as MASTER_ORGAN_NAME",
                "dbo.FN_GetOuName(M1.EXEC_ORGAN_C, 3) as EXEC_ORGAN_NAME",
                "D2.DELAY_KIND",
                "D5.ENGNEER_STAGE",
                "isnull(D2.PRG_OFFSET, 0) as PRG_OFFSET"
            }));

            sql.AppendLine("and isnull(D2.PRG_OFFSET, 0) < 0");

            // 主管機關
            if (!string.IsNullOrEmpty(model.MASTER_DEPT))
            {
                sql.AppendLine("and M1.MASTER_ORGAN_C = @MASTER_DEPT");
            }

            // 執行機關
            if (!string.IsNullOrEmpty(model.EXEC_DEPT))
            {
                sql.AppendLine("and M1.EXEC_ORGAN_C = @EXEC_DEPT");
            }

            // 計畫年度
            if (!string.IsNullOrEmpty(model.PROJECT_YEAR))
            {
                sql.AppendLine("and M1.PROJECT_YEAR = @PROJECT_YEAR");
            }

            // 計畫名稱
            if (!string.IsNullOrEmpty(model.PROJECT_NAME))
            {
                sql.AppendLine("and M1.PROJECT_NAME like '%' + @PROJECT_NAME + '%'");
            }

            // 是否只有主辦權限
            if (model.IS_HAND_ROLE)
            {
                string orgId = profile.GetLoginUser().ORG_ID;
                sql.AppendLine($"and (M1.MASTER_ORGAN_C = '{orgId}' or M1.EXEC_ORGAN_C = '{orgId}')");
            }

            // 檢核點進度落後60天以上案件
            if (model.RPT_TYPE == "4")
            {
                sql.Append(@$" AND [dbo].[FN_GET_DELAY_DAYS] (M1.PROJECT_NO, 
                                   YEAR( DATEADD(day, -1, DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0)))-1911,    
                                   Right('00' + Cast(MONTH( DATEADD(day, -1, DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0))) as varchar),2), 
                                   DATEADD(day, -1, DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0)))>=60");
            }

            model.CRT_USER = UserId;
            return (await ExecuteQueryAsync<DelayPlanModel>(sql.ToString(), model)).ToList();
        }

        /// <summary>
        /// 取得落後清單
        /// </summary>
        /// <param name="projectNos">計畫編號清單</param>
        /// <returns></returns>
        public async Task<List<(string PROJECT_NO, int DATA_YEAR, int DATA_MONTH)>> GetDelayCausals(List<string> projectNos)
        {
            string sql = @"select 
	                            PROJECT_NO,
                                Cast(DATA_YEAR as int) DATA_YEAR,
	                            Cast(DATA_MONTH as int) DATA_MONTH
                            from PROJECT_DELAY_CAUSAL (nolock)
                            where PROJECT_NO in @projectNos";
            return (await ExecuteQueryAsync<(string, int, int)>(sql, new { projectNos })).ToList();
        }

        /// <summary>
        /// 取得落後60天以上的計畫清單
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetDayOffsetProjNos()
        {
            string sql = @"select distinct 
	                            M1.PROJECT_NO
                            from PROJECT_CHECKITEM (nolock) M1
                            inner join (
	                            select 
		                            PROJECT_NO, 
                                    Max(ACTUAL_ENDDATE) as MAX_ACTUAL_ENDDATE
	                            from PROJECT_CHECKITEM (nolock)
	                            group by PROJECT_NO
                            ) as M2 on M1.PROJECT_NO = M2.PROJECT_NO and M1.ACTUAL_ENDDATE = M2.MAX_ACTUAL_ENDDATE
                            where DATEDIFF(DAY, ESTIMATED_ENDDATE, ACTUAL_ENDDATE) >= 60";
            return (await ExecuteQueryAsync<string>(sql)).ToList();
        }
        #endregion 落後案件查詢
    }
}
