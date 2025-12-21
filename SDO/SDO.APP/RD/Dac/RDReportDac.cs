using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SDO.APP.IPC.Models.Statistics;
using SDO.APP.RD.Models.Report;
using SDO.Models;
using SDO.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class RDReportDac : Dac, IRDReportDacDac
    {
        public RDReportDac(IConnectionControlCenter connectionControlCenter,
           IHttpContextAccessor httpContextAccessor,
           ISqlTrace trace,
           IUserProfile profile,
           IParameterAdaptor ParameterAdaptor,
           IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 季委託研究計畫列管表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<SeasonReportModel>> GetSeason(ReportQueryModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine($@"
                        SELECT 
                               M1.PLAN_NAME,
                               M1.PLAN_START_DATE,
                               M1.PLAN_END_DATE,
                               SUM(M1.PUBLIC_MONEY+M1.FUND_MONEY+M1.CENTER_MONEY+M1.OTHER_MONEY) AS SUM_MONEY,
                               dbo.FN_GetOuName(M1.OU_ID, 3) AS OU_NAME,
                               M2.EXECUTION_DESC AS EXECUTION_DESC
                        FROM RD_RESEARCH_BASIC M1
						LEFT JOIN (
                                SELECT  
	                                PLAN_NO,EXECUTION_DESC,
	                                ROW_NUMBER() OVER ( PARTITION BY PLAN_NO ORDER BY SEQ DESC ) AS RN
                                FROM RD_RES_POLICY_INDEX (NOLOCK)
					            WHERE STATUS <> '1'
                        ) M2 ON M1.PLAN_NO = M2.PLAN_NO AND RN = 1
                        WHERE 1 = 1");

            if (!string.IsNullOrEmpty(model.PLAN_YEAR))
            {
                sql.AppendLine("and M1.PLAN_YEAR = @PLAN_YEAR");
            }
            if (!string.IsNullOrEmpty(model.PLAN_NAME))
            {
                sql.AppendLine("and M1.PLAN_NAME = @PLAN_NAME");
            }
            if (!string.IsNullOrEmpty(model.PLAN_NO))
            {
                sql.AppendLine("and M1.PLAN_NO = @PLAN_NO");
            }
            if (!string.IsNullOrEmpty(model.OU_ID))
            {
                sql.AppendLine("and M1.OU_ID = @OU_ID");
            }
            if (!string.IsNullOrEmpty(model.SEASON_TYPE))
            {
                sql.AppendLine("and M2.SEASON_TYPE = @SEASON_TYPE");
            }
            sql.AppendLine("GROUP BY PLAN_NAME,PLAN_START_DATE,PLAN_END_DATE,OU_ID,M2.EXECUTION_DESC");
            return (await ExecuteQueryAsync<SeasonReportModel>(sql.ToString(), model, RDDBKey)).ToList();
        }

        /// <summary>
        /// 本府委託研究計畫成果及運用情形調查列管表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<PlanResultModel>> GetPlanResult(ReportQueryModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine($@"
                        SELECT 
                               M1.PLAN_NO,
                               M1.PLAN_NAME,
                               M1.PLAN_START_DATE,
                               M1.PLAN_END_DATE,
                               SUM(M1.PUBLIC_MONEY+M1.FUND_MONEY+M1.CENTER_MONEY+M1.OTHER_MONEY) AS SUM_MONEY,
                               dbo.FN_GetOuName(M1.OU_ID, 3) AS OU_NAME,
                               M2.SITUATION_TYPE AS SITUATION_TYPE
                        FROM RD_RESEARCH_BASIC M1
                        LEFT JOIN RD_RES_SITUATION M2 ON M1.PLAN_NO = M2.PLAN_NO
                        WHERE 1 = 1");

            if (!string.IsNullOrEmpty(model.PLAN_YEAR))
            {
                sql.AppendLine("and M1.PLAN_YEAR = @PLAN_YEAR");
            }
            if (!string.IsNullOrEmpty(model.PLAN_NAME))
            {
                sql.AppendLine("and M1.PLAN_NAME = @PLAN_NAME");
            }
            if (!string.IsNullOrEmpty(model.PLAN_NO))
            {
                sql.AppendLine("and M1.PLAN_NO = @PLAN_NO");
            }
            if (!string.IsNullOrEmpty(model.OU_ID))
            {
                sql.AppendLine("and M1.OU_ID = @OU_ID");
            }
            if (!string.IsNullOrEmpty(model.SEASON_TYPE))
            {
                sql.AppendLine("and M2.SEASON_TYPE = @SEASON_TYPE");
            }
            if (!string.IsNullOrEmpty(model.PLAN_START_DATE))
            {
                sql.AppendLine("and M1.PLAN_START_DATE >= @PLAN_START_DATE");
            }
            if (!string.IsNullOrEmpty(model.PLAN_END_DATE))
            {
                sql.AppendLine("and M1.PLAN_END_DATE <= @PLAN_END_DATE");
            }
            if (!string.IsNullOrEmpty(model.SITUATION_TYPE))
            {
                sql.AppendLine("and M2.SITUATION_TYPE = @SITUATION_TYPE");
            }
            sql.AppendLine("GROUP BY M1.PLAN_NO,PLAN_NAME,PLAN_START_DATE,PLAN_END_DATE,OU_ID,M2.SITUATION_TYPE");
            return (await ExecuteQueryAsync<PlanResultModel>(sql.ToString(), model, RDDBKey)).ToList();
        }

        /// <summary>
        /// 委託研究計畫執行情形調查表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<PlanExecutionModel>> GetPlanExecution(ReportQueryModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine($@"
                        SELECT PLAN_YEAR,
                               dbo.FN_GetOuName(OU_ID, 3) AS OU_NAME,
                               PLAN_NAME,
                               ENTRUST_UNIT_NAME,
                               SUM(PUBLIC_MONEY+FUND_MONEY+CENTER_MONEY+OTHER_MONEY) AS SUM_MONEY,
                               PLAN_START_DATE,
                               PLAN_END_DATE,
                               MID_REPORT_YM,
                               FINAL_REPORT_YM
                        FROM RD_RESEARCH_BASIC M1
                        LEFT JOIN RD_RES_SITUATION M2 ON M1.PLAN_NO = M2.PLAN_NO
                        WHERE 1 = 1");

            if (!string.IsNullOrEmpty(model.PLAN_YEAR))
            {
                sql.AppendLine("and M1.PLAN_YEAR = @PLAN_YEAR");
            }
            if (!string.IsNullOrEmpty(model.PLAN_NAME))
            {
                sql.AppendLine("and M1.PLAN_NAME = @PLAN_NAME");
            }
            if (!string.IsNullOrEmpty(model.PLAN_NO))
            {
                sql.AppendLine("and M1.PLAN_NO = @PLAN_NO");
            }
            if (!string.IsNullOrEmpty(model.OU_ID))
            {
                sql.AppendLine("and M1.OU_ID = @OU_ID");
            }
            if (!string.IsNullOrEmpty(model.SITUATION_TYPE))
            {
                sql.AppendLine("and M2.SITUATION_TYPE = @SITUATION_TYPE");
            }
           
            sql.AppendLine($@"GROUP BY PLAN_YEAR,PLAN_NAME,ENTRUST_UNIT_NAME,
                                       OU_ID,MID_REPORT_YM,FINAL_REPORT_YM,
                                       PLAN_START_DATE,PLAN_END_DATE");

            return (await ExecuteQueryAsync<PlanExecutionModel>(sql.ToString(), model, RDDBKey)).ToList();
        }

        /// <summary>
        /// 參採情形總表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ParticipatingModel>> GetParticipating(ReportQueryModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine($@"
                        SELECT M1.PLAN_YEAR,
	                           dbo.FN_GetOuName(M1.OU_ID, 3) AS OU_NAME,
	                           M1.PLAN_NAME,
	                           M1.PLAN_START_DATE,
	                           M1.PLAN_END_DATE,
	                           SUM(M1.PUBLIC_MONEY+M1.FUND_MONEY+M1.CENTER_MONEY+M1.OTHER_MONEY) AS SUM_MONEY,
	                           M1.ENTRUST_UNIT_NAME,
	                           M1.RESEARCH_STATUS,
	                           M2.SITUATION_TYPE AS SITUATION_TYPE,
	                           M2.SITUATION_DESC AS SITUATION_DESC
                        FROM RD_RESEARCH_BASIC M1
                        LEFT JOIN RD_RES_SITUATION M2 ON M1.PLAN_NO = M2.PLAN_NO
                        WHERE 1 = 1");

            if (!string.IsNullOrEmpty(model.PLAN_YEAR))
            {
                sql.AppendLine("and M1.PLAN_YEAR = @PLAN_YEAR");
            }
            if (!string.IsNullOrEmpty(model.PLAN_NAME))
            {
                sql.AppendLine("and M1.PLAN_NAME = @PLAN_NAME");
            }
            if (!string.IsNullOrEmpty(model.PLAN_NO))
            {
                sql.AppendLine("and M1.PLAN_NO = @PLAN_NO");
            }
            if (!string.IsNullOrEmpty(model.OU_ID))
            {
                sql.AppendLine("and M1.OU_ID = @OU_ID");
            }
            if (!string.IsNullOrEmpty(model.SITUATION_TYPE))
            {
                sql.AppendLine("and M2.SITUATION_TYPE = @SITUATION_TYPE");
            }

            sql.AppendLine($@"GROUP BY M1.PLAN_YEAR,M1.OU_ID,M1.PLAN_NAME,
                                       M1.PLAN_START_DATE,M1.PLAN_END_DATE,M1.ENTRUST_UNIT_NAME,
                                       M1.RESEARCH_STATUS,M2.SITUATION_TYPE,M2.SITUATION_DESC");

            return (await ExecuteQueryAsync<ParticipatingModel>(sql.ToString(), model, RDDBKey)).ToList();
        }

        /// <summary>
        /// 續列管委託研究計畫成果及運用情形調查表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<PlanExecutionSurveyModel> GetPlanExecutionSurvery(ReportQueryModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine($@"
                        SELECT  dbo.FN_GetOuName(M1.OU_ID, 3) AS OU_NAME,
                                M1.PLAN_NAME,
		                        M1.ENTRUST_UNIT_NAME,
		                        M1.RESEARCH_NAME,
                                M1.PLAN_START_DATE,
                                M1.PLAN_END_DATE,
                                SUM(M1.PUBLIC_MONEY+M1.FUND_MONEY+M1.CENTER_MONEY+M1.OTHER_MONEY) AS SUM_MONEY,      
                                M2.SITUATION_TYPE AS SITUATION_TYPE,
		                        M1.PLAN_CAUSE,
		                        M1.PLAN_EXPECTED,
		                        M2.SITUATION_DESC,
		                        M1.CRT_DATE
                        FROM RD_RESEARCH_BASIC M1
                        LEFT JOIN RD_RES_SITUATION M2 ON M1.PLAN_NO = M2.PLAN_NO
                        WHERE 1 = 1");

            if (!string.IsNullOrEmpty(model.PLAN_YEAR))
            {
                sql.AppendLine("and M1.PLAN_YEAR = @PLAN_YEAR");
            }
            if (!string.IsNullOrEmpty(model.PLAN_NAME))
            {
                sql.AppendLine("and M1.PLAN_NAME = @PLAN_NAME");
            }
            if (!string.IsNullOrEmpty(model.PLAN_NO))
            {
                sql.AppendLine("and M1.PLAN_NO = @PLAN_NO");
            }
            if (!string.IsNullOrEmpty(model.OU_ID))
            {
                sql.AppendLine("and M1.OU_ID = @OU_ID");
            }
            if (!string.IsNullOrEmpty(model.SEASON_TYPE))
            {
                sql.AppendLine("and M2.SEASON_TYPE = @SEASON_TYPE");
            }
            if (!string.IsNullOrEmpty(model.PLAN_START_DATE))
            {
                sql.AppendLine("and M1.PLAN_START_DATE >= @PLAN_START_DATE");
            }
            if (!string.IsNullOrEmpty(model.PLAN_END_DATE))
            {
                sql.AppendLine("and M1.PLAN_END_DATE <= @PLAN_END_DATE");
            }
            if (!string.IsNullOrEmpty(model.SITUATION_TYPE))
            {
                sql.AppendLine("and M2.SITUATION_TYPE = @SITUATION_TYPE");
            }

            sql.AppendLine($@"GROUP BY M1.OU_ID,M1.PLAN_NAME,M1.ENTRUST_UNIT_NAME,
                                       M1.RESEARCH_NAME,M1.PLAN_START_DATE,M1.PLAN_END_DATE,
                                       M2.SITUATION_TYPE,M1.PLAN_CAUSE,M1.PLAN_EXPECTED,
                                       M2.SITUATION_DESC,M1.CRT_DATE");

            return (await ExecuteQueryFirstOrDefaultAsync<PlanExecutionSurveyModel>(sql.ToString(), model, RDDBKey));
        }

    }
}
