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
    public class ProjectExecutionDac : Dac, IProjectExecutionDac
    {
        public ProjectExecutionDac(IConnectionControlCenter connectionControlCenter,
           IHttpContextAccessor httpContextAccessor,
           ISqlTrace trace,
           IUserProfile profile,
           IParameterAdaptor ParameterAdaptor,
           IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
            
        }

        /// <summary>
        /// 取得執行情形填報清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ResPolicyListQueryModel>> GetRDResPolicyList(ResPolicyListQueryModel model)
        {
            string sql = @"
                        select
                            M1.SEQ,
	                        dbo.FN_GetSetParam('POLICY_KIND', M1.POLICY_KIND) as POLICY_KIND,
	                        M1.POLICY_INDEX_DESC,
	                        M1.RES_FINISH_DATE,
	                        dbo.FN_GetSetParam('POLICY_STATUS', M1.STATUS) as STATUS,
	                        M1.STATUS as POLICY_STATUS,
	                        M2.RESEARCH_STATUS
                        from
	                        RD_RES_POLICY_INDEX M1(nolock)
                        inner join 
	                        RD_RESEARCH_BASIC M2(nolock)
                        on 
	                        M1.PLAN_NO = M2.PLAN_NO
                        where 
	                        M1.PLAN_NO = @PLAN_NO";

            return (await ExecuteQueryAsync<ResPolicyListQueryModel>(sql, model, RDDBKey)).ToList();
        }

        /// <summary>
        /// 取得執行情形填報明細
        /// </summary>
        /// <param name="SEQ"></param>
        /// <returns></returns>
        public async Task<ResPolicyIndexModel> GetRDResPolicyIndex(int SEQ)
        {
            string sql = @"
                        select
	                        M1.PLAN_NO,
	                        M1.ENTRUST_UNIT_NAME,
	                        M1.RESEARCH_NAME,
	                        M1.OU_ID,
	                        M2.STATUS,
	                        M2.PLAN_YEAR,
	                        M2.SEASON_TYPE,
	                        M2.POLICY_INDEX_DESC,
	                        M2.POLICY_KIND,
	                        M2.RES_FINISH_DATE,
	                        M2.PROGRESS_TYPE,
	                        M2.EXECUTION_DESC,
	                        M2.BEHIND_REASON,
	                        M2.SOLUTIONS
                        from
	                        RD_RESEARCH_BASIC M1(nolock)
                        inner join
	                        RD_RES_POLICY_INDEX M2(nolock)
                        on
	                        M1.PLAN_NO = M2.PLAN_NO
                        where 
	                        M2.SEQ = @SEQ";

            return await ExecuteQueryFirstOrDefaultAsync<ResPolicyIndexModel>(sql, new { SEQ }, RDDBKey);
        }

        /// <summary>
        /// 修改執行情形
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> SaveRDResPolicyIndex(ResPolicyIndexModel model)
        {
            string sql = $@"
                        update
                            RD_RES_POLICY_INDEX
                        set
                            PROGRESS_TYPE = @PROGRESS_TYPE,
                            EXECUTION_DESC = @EXECUTION_DESC,
                            BEHIND_REASON = @BEHIND_REASON,
                            SOLUTIONS = @SOLUTIONS,
                            MDF_USER = @MDF_USER,
                            MDF_DATE = {DTNow}
                        where 
	                        SEQ = @SEQ";

            return await ExecuteCommandAsync(sql, model, RDDBKey);

        }
    }
}
