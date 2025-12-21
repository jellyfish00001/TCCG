using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SDO.APP.IPC.Models.Statistics;
using SDO.Models;
using SDO.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class InnStatisticsDac : Dac, IInnStatisticsDac
    {
        public InnStatisticsDac(IConnectionControlCenter connectionControlCenter,
           IHttpContextAccessor httpContextAccessor,
           ISqlTrace trace,
           IUserProfile profile,
           IParameterAdaptor ParameterAdaptor,
           IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        public async Task<List<PlanListModel>> GetPlanList(InnStatisticsModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine($@"
                    SELECT M1.INN_YEAR,
                           M1.OU_ID,
                           M1.INN_PLAN_NO,
                           M1.INN_PLAN_NAME,
                           M1.INN_DESCRIPTION,
                           M1.[GROUP],
                           M1.IDEA_CONTENT,
                           M1.EXPECT_BENEFIT,
                           M1.SPONSOR_ORG,
                           M1.SPONSOR_UNIT,
                           M1.SPONSOR_TITLE,
                           M1.SPONSOR_NAME,
                           M1.SPREAD_IDEA_YN,
                           M1.REJECT_YN,
                           M1.ORIGINATE_YN,
                           M1.CONTACT_NAME,
                           M1.CONTACT_TEL,
                           M1.CONTACT_EMAIL,
                           M1.CONTACT_TITLE,
                           M1.CONTACT_ORG,
	                       M2.PROPOSAL_KIND,
	                       M2.PROPOSAL_TYPE_ID,
	                       M3.CODE_VALUE AS PROPOSALTYPE_MAIN,
                           M4.SET_VALUE AS SPONSOR_TYPE,
                           M5.SET_VALUE AS SPONSOR_SEX,
                           dbo.FN_GetOuName(SPONSOR_ORG, 3) AS OU_NAME
                    FROM INN_BASIC AS M1
                        LEFT JOIN INN_PROJECT_PROPOSALTYPE AS M2 
                            ON M1.INN_PLAN_NO = M2.INN_PLAN_NO
                        LEFT JOIN INN_PROJECT_TITLE AS M3 
                            ON M2.PROPOSAL_TYPE_ID = M3.ID
                        INNER JOIN SET_PARAM AS M4
							ON M4.SET_TYPE=M1.SPONSOR_TYPE AND M4.SET_ITEM = 'SPONSORTYPE'
                        INNER JOIN SET_PARAM AS M5
							ON M5.SET_TYPE=M1.SPONSOR_SEX AND M5.SET_ITEM ='SPONSOR_SEX'
                    WHERE 
						M2.PROPOSAL_KIND = 1");

            if (!string.IsNullOrEmpty(model.INN_YEAR))
            {
                sql.AppendLine("and M1.INN_YEAR = @INN_YEAR");
            }
            return (await ExecuteQueryAsync<PlanListModel>(sql.ToString(), model, INNDBKey)).ToList();
        }

        public async Task<List<InnProjectProposalTypeModel>> GetPlanProposalTypeSub(InnStatisticsModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine($@"
                    SELECT M1.INN_PLAN_NO,
	                       M2.CODE_VALUE AS PROPOSAL_TYPE_NAME
                    FROM INN_PROJECT_PROPOSALTYPE M1
                        LEFT JOIN INN_PROJECT_TITLE　AS M2
                            ON M1.PROPOSAL_TYPE_ID = M2.ID
                    WHERE M1.PROPOSAL_KIND = 2");

            if (!string.IsNullOrEmpty(model.INN_YEAR))
            {
                sql.AppendLine("and M2.YEAR = @INN_YEAR");
            }

            return (await ExecuteQueryAsync<InnProjectProposalTypeModel>(sql.ToString(), model, INNDBKey)).ToList();
        }

        public async Task<List<DeptStatisticsModel>> GetDeptStatistics(InnStatisticsModel model)
        {

            StringBuilder sql = new StringBuilder();
            sql.AppendLine($@"
                    SELECT AA.* , ROW_NUMBER() OVER (PARTITION BY AA.DEPT_TYPE ORDER BY AA.SPONSOR_ORG ASC)AS SQE
                    FROM (
                    SELECT dbo.FN_GetOuName(SPONSOR_ORG, 3) AS SPONSOR_ORG,
		                    (CASE WHEN dbo.FN_GetOuName(SPONSOR_ORG, 3) LIKE '%區公所%' THEN '區公所' 
										                    ELSE '一二級機關' END) AS DEPT_TYPE,
                                               M3.CODE_VALUE AS PROPOSALTYPE_MAIN,
                                               COUNT(M1.INN_PLAN_NO) AS PROPOSALTYPE_COUNT
                                        FROM INN_BASIC AS M1
                                            INNER JOIN INN_PROJECT_PROPOSALTYPE AS M2 
                                                ON M1.INN_PLAN_NO = M2.INN_PLAN_NO
                                            INNER JOIN INN_PROJECT_TITLE AS M3 
                                                ON M2.PROPOSAL_TYPE_ID = M3.ID
                                        WHERE M2.PROPOSAL_KIND = 1");

            if (!string.IsNullOrEmpty(model.INN_YEAR))
            {
                sql.AppendLine("and M1.INN_YEAR = @INN_YEAR");
            }

            sql.AppendLine($@"GROUP BY M1.SPONSOR_ORG, M3.CODE_VALUE) AA");

            return (await ExecuteQueryAsync<DeptStatisticsModel>(sql.ToString(), model, INNDBKey)).ToList();
        }


        public async Task<List<YearStatisticsModel>> GetYearStatistics(InnStatisticsModel model)
        {

            StringBuilder sql = new StringBuilder();
            sql.AppendLine($@"
                    SELECT  INN_YEAR,
                            COUNT(CASE WHEN dbo.FN_GetOuName(SPONSOR_ORG, 3) LIKE '%區公所%' THEN 1 END) AS COUNT_SPONSOR_DISTRICT_OFFICE,
                            COUNT(CASE WHEN dbo.FN_GetOuName(SPONSOR_ORG, 3) NOT LIKE '%區公所%' THEN 1 END) AS COUNT_SPONSOR_ORG,
                            COUNT(CASE WHEN SPONSOR_TYPE = 1 THEN 1 END) AS COUNT_SPONSOR_TYPE_1,
                            COUNT(CASE WHEN SPONSOR_TYPE = 2 THEN 1 END) AS COUNT_SPONSOR_TYPE_2,
                            COUNT(CASE WHEN SPONSOR_SEX = 1 THEN 1 END) AS COUNT_SPONSOR_SEX_1,
                            COUNT(CASE WHEN SPONSOR_SEX = 2 THEN 1 END) AS COUNT_SPONSOR_SEX_2,
                            COUNT(INN_PLAN_NO) AS PLAN_SUM 
                    FROM [dbo].[INN_BASIC]
                    WHERE 1 = 1");
            if (!string.IsNullOrEmpty(model.INN_YEAR))
            {
                sql.AppendLine($@"and INN_YEAR <= @INN_YEAR 
                                  GROUP BY INN_YEAR
                                  ORDER BY INN_YEAR;");
            }

            return (await ExecuteQueryAsync<YearStatisticsModel>(sql.ToString(), model, INNDBKey)).ToList();
        }

    }
}
