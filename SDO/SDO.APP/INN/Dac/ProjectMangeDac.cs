using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using SDO.APP.INN.Models.ProjectManage;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class ProjectManageDac : Dac, IProjectManageDac
    {
        public ProjectManageDac(IConnectionControlCenter connectionControlCenter,
          IHttpContextAccessor httpContextAccessor,
          ISqlTrace trace,
          IUserProfile profile,
          IParameterAdaptor ParameterAdaptor,
          IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {

        }

        /// <summary>
        /// 抓取提案
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ProjectManageModel>> GetProjectManage(ProjectManageQueryModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(@"SELECT
                                    M1.INN_PLAN_ID,
                                    M1.INN_YEAR,
                                    M1.INN_PLAN_NO,
                                    M1.INN_PLAN_NAME,
                                    M1.[GROUP],
                                    dbo.FN_GetOuName(SPONSOR_ORG, 3) AS SPONSOR_ORG,
                                    M1.SPONSOR_NAME,
                                    M1.SPONSOR_UNIT,
                                    M1.SPONSOR_SEX,
                                    M1.OU_ID,
                                    dbo.FN_GetOuName(OU_ID, 3) AS OU_NAME,
                                    D1.CODE_VALUE 
                               FROM
                                    INN_BASIC M1(NOLOCK)
                                    LEFT JOIN INN_PROJECT_PROPOSALTYPE M2 (NOLOCK)
                                        ON M1.INN_PLAN_NO = M2.INN_PLAN_NO 
                                        AND M2.PROPOSAL_KIND = '1'
                                    LEFT JOIN INN_PROJECT_TITLE D1 (NOLOCK)
                                        ON M2.PROPOSAL_TYPE_ID = D1.ID
                               WHERE 1 = 1");

            //提案年度
            if (model.INN_YEAR != null)
            {
                sql.AppendLine("and INN_YEAR = @INN_YEAR");
            }
            //提案編號
            sql.AppendLine(FuzzySearch(model.INN_PLAN_NO, "M1.INN_PLAN_NO"));

            //提案名稱
            sql.AppendLine(FuzzySearch(model.INN_PLAN_NAME, "INN_PLAN_NAME"));
            
            if (!string.IsNullOrEmpty(model.PROPOSAL_TYPE))
            {
                sql.AppendLine(" and D1.ID = @PROPOSAL_TYPE");
            }
            if (!string.IsNullOrEmpty(model.GROUP))
            {
                sql.AppendLine(" and [GROUP] = @GROUP");
            }
            if (!string.IsNullOrEmpty(model.SPONSOR_SEX))
            {
                sql.AppendLine(" and SPONSOR_SEX = @SPONSOR_SEX");
            }
            if (!string.IsNullOrEmpty(model.OU_ID))
            {
                sql.AppendLine(" and OU_ID = @OU_ID");
            }
            if (!string.IsNullOrEmpty(model.CRT_USER))
            {
                sql.AppendLine(" and M1.CRT_USER = @CRT_USER");
            }

            return (await ExecuteQueryAsync<ProjectManageModel>(sql.ToString(), model, INNDBKey)).ToList();
        }

    }
}
