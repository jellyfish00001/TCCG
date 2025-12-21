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
    public class SCApplicationDac : Dac, ISCApplicationDac
    {
        public SCApplicationDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 取得系統路徑
        /// </summary>
        /// <param name="apId">系統代碼</param>
        /// <returns></returns>
        public async Task<string> GetPrgPath(string apId)
        {
            string sql = @"select 
                               PRG_PATH
                           from SCAPPLICATIONM (NOLOCK)
                           where AP_VER = '2.0' and AP_ID = @apId";
            return (await ExecuteQueryAsync<string>(sql, param: new { apId }, DB: SCDBKey)).FirstOrDefault();
        }


        /// <summary>
        /// 新增 Session資訊
        /// </summary>
        /// <param name="model"></param>
        public void InsertScSession(SCSessionModel model)
        {
            string sql = @"insert into SCSESSIONM 
                                (AP_ID, USR_COMP_ID, USR_ID, SESSION_ID, CRT_TIME)
                            values 
                                (@AP_ID, @USR_COMP_ID, @USR_ID, @SESSION_ID, GETDATE())";
            ExecuteCommand(sql, param: model, DB: SCDBKey);
        }

        /// <summary>
        /// 取得先期計畫數量
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public async Task<int> GetPWSSDCnt(string orgId)
        {
            string sql = $@"select COUNT(*)
                            from 
                            (
	                            select PlanId,PlanYear from PWSSDPlanMain
	                            where OU_ID = @orgId  
	                            UNION
                                select PlanId,PlanYear from PWSSDPlanMain
	                            where CreateOrgOuId = @orgId 
                                UNION 
	                            select PlanId,PlanYear from PWSSDPlanMain
	                            where CreateOrgOuId IN (
		                            select
			                            A.F_GROUP_ID
		                            from {SC30_M}.[GPREL_GRP_GRPM] A
		                            left join {SC30_M}.[SCORG_UNITM] B on B.OU_ID = A.F_GROUP_ID
		                            where T_GROUP_ID = @orgId
			                            and REL_KIND = '1'
			                            and LINK_KIND = '1'
			                            and LINK_DIST = '1'
			                            and B.OU_KIND = '2'
	                            )
                            ) as M
                            where PlanYear = (select max(PlanYear) from PWSSDYearSet (nolock))";
            return (await ExecuteQueryAsync<int>(sql, param: new { orgId }, DB: RISDBKey)).FirstOrDefault();
        }

        /// <summary>
        /// 取得研究發展最大年度
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetRdMaxYear()
        {
            string sql = @"select max(PlanYear) from RDResearchBasic (nolock)";
            return (await ExecuteQueryAsync<string>(sql, DB: RISDBKey)).FirstOrDefault();
        }

        /// <summary>
        /// 取得委託研究數量
        /// </summary>
        /// <param name="maxYear"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public async Task<int> GetRDRPMCnt(string maxYear, string orgId)
        {
            string sql = $@"select
	                            Count(M1.PlanId)
                            from RDResearchBasic M1 (nolock) 
                            left join RDResSituation D1 (nolock) on M1.PlanId = D1.PlanId 
                            left join {SC30_M}.[SCREL_GRP_GRPMV] as D2 on M1.OU_ID = D2.F_GROUP_ID and D2.REL_KIND = '1' and D2.LINK_KIND = '1' and D2.LINK_DIST = '1' and D2.T_IS_ENABLE = 'Y' 
                            where (M1.OU_ID = @orgId or D2.T_GROUP_ID = @orgId)
	                            and M1.PlanYear = @maxYear  
	                            and M1.RevokedYN = 'N'";
            return (await ExecuteQueryAsync<int>(sql, param: new { maxYear, orgId }, DB: RISDBKey)).FirstOrDefault();
        }

        /// <summary>
        /// 取得創新提案數量
        /// </summary>
        /// <param name="maxYear"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public async Task<int> GetRDWIPCnt(string maxYear, string orgId)
        {
            string sql = @"select 
	                            count(OU_ID)
                            from RDInnBasic (nolock)
                            where OU_ID = @orgId
	                            and RDInnYear = @maxYear";
            return (await ExecuteQueryAsync<int>(sql, param: new { maxYear, orgId }, DB: RISDBKey)).FirstOrDefault();
        }

    }
}
