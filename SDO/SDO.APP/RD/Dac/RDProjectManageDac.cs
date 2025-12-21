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
    public class RDProjectManageDac : Dac, IRDProjectManageDac
    {
        public RDProjectManageDac(
            IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {}

        /// <summary>
        /// 委託研究計畫管理(初始頁面撈全部資料、帶搜尋條件篩選資料)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<RDProjectManageModel>> GetRDProjectManage(RDProjectManageQueryModel model)
        {
            StringBuilder sql = new StringBuilder();
            
            sql.AppendLine(@$"
                        select
	                        scUser.USR_NAME as CRT_USER_NAME,
	                        M1.PLAN_ID as PLAN_ID,
	                        M1.PLAN_NO as PLAN_NO,
	                        M1.PLAN_NAME as PLAN_NAME,
	                        M1.ENTRUST_UNIT_NAME as ENTRUST_UNIT_NAME,
	                        M1.PLAN_START_DATE as PLAN_START_DATE,
	                        M1.PLAN_END_DATE as PLAN_END_DATE,  
	                        M1.CONTACT_NAME as CONTACT_NAME,
	                        M1.OU_ID as OU_ID,
	                        dbo.FN_GetOuName(M1.OU_ID, 3) as EXEC_ORG_NAME,
	                        M2.OU_SORT_ORDER AS EXEC_ORG_ORDER
                        from
	                        RD_RESEARCH_BASIC M1(nolock)
                        left join
                            {SC30_M}.SCORG_UNITM as M2
                        on
                            M1.OU_ID = M2.OU_ID 
                        left join
                            {SC30_M}.SCUSERM as scUser
                        on
                            M1.CRT_USER = scUser.USR_ID
                        where M1.REVOKED_YN <> 'D'
                      ");

            // 計畫年度
            if (!string.IsNullOrEmpty(model.PLAN_YEAR))
            {
                sql.AppendLine(" and M1.PLAN_YEAR = @PLAN_YEAR");
            }

            // 計畫編號，多筆模糊查詢
            sql.AppendLine(FuzzySearch(model.PLAN_NO, "M1.PLAN_NO"));
            // 計畫名稱，多筆模糊查詢
            sql.AppendLine(FuzzySearch(model.PLAN_NAME, "M1.PLAN_NAME"));

            // 受託單位
            if (!string.IsNullOrEmpty(model.ENTRUST_UNIT_NAME))
            {
                sql.AppendLine(" and M1.ENTRUST_UNIT_NAME like @ENTRUST_UNIT_NAME");
            }

            // 機關編號
            if (!string.IsNullOrEmpty(model.OU_ID))
            {
                sql.AppendLine(" and M1.OU_ID = @OU_ID");
            }

            sql.AppendLine(" order by M1.PLAN_NO desc");

            return (await ExecuteQueryAsync<RDProjectManageModel>(sql.ToString(), model, RDDBKey)).ToList();
        }
    }
}
