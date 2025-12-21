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
    /// <summary>
    /// 結案成果填報
    /// </summary>
    public class ProjectSituationDac : Dac, IProjectSituationDac
    {
        public ProjectSituationDac(
           IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 取得結案成果填報結果
        /// </summary>
        /// <param name="PLAN_NO">計畫編號</param>
        /// <returns></returns>
        public async Task<ResSituationModel> GetRDResSituation(string PLAN_NO)
        {
            string sql = @"
                        select
	                        M1.PLAN_NO,
	                        M1.SITUATION_STATUS,
	                        M1.SITUATION_TYPE,
	                        M1.SITUATION_DESC,
	                        M1.CLOSING_DATE,
	                        M2.RD_RES_POLICY_INDEX_COUNT,
	                        COALESCE(M3.STATUS_3_COUNT, 0) AS STATUS_3_COUNT
                        from
	                        RD_RES_SITUATION M1(nolock)
                        right join (
                            select
                                PLAN_NO,
                                COUNT(*) AS RD_RES_POLICY_INDEX_COUNT
                            from
                                RD_RES_POLICY_INDEX
                            where
                                PLAN_NO = @PLAN_NO
                            group by
                                PLAN_NO
                        ) as M2
                        on
                            M1.PLAN_NO = M2.PLAN_NO
                        left join (
                            select
                                PLAN_NO,
                                COUNT(*) AS STATUS_3_COUNT
                            from
                                RD_RES_POLICY_INDEX
                            where
                                PLAN_NO = @PLAN_NO
                            and
                                STATUS = '3'
                            group by
                                PLAN_NO
                        ) as M3
                        on
                            M3.PLAN_NO = M2.PLAN_NO
                        where
                            M2.PLAN_NO = @PLAN_NO";

            return await ExecuteQueryFirstOrDefaultAsync<ResSituationModel>(sql, new { PLAN_NO }, RDDBKey);
        }

        /// <summary>
        /// 取得續列管一年內參採情形
        /// </summary>
        /// <param name="PLAN_NO">計畫編號</param>
        /// <returns></returns>
        public async Task<ResSituationModel> GetRDResSituaContinue(string PLAN_NO)
        {
            string sql = @"
                        select
                            dbo.FN_GetSetParam('RDSituationType', M1.SITUATION_TYPE) as SITUATION_TYPE,
	                        M1.SITUATION_DESC,
                            M1.SITUATION_STATUS,
	                        M2.SITUATION_TYPE as CONTINUE_SITUAITON_TYPE,
                            M2.CONTINUE_SITUAITON_DESC,
                            M2.PLAN_NO,
                            M2.SITUACONTINUE_STATUS
                        from
	                        RD_RES_SITUATION as M1(nolock)
                        left join
	                        RD_RES_SITUACONTINUE as M2(nolock)
                        on
	                        M1.PLAN_NO = M2.PLAN_NO
                        where
                            M1.PLAN_NO = @PLAN_NO";

            return await ExecuteQueryFirstOrDefaultAsync<ResSituationModel>(sql, new { PLAN_NO }, RDDBKey);
        }

        /// <summary>
        /// 儲存參採情形/結案成果填報結果
        /// </summary>
        /// <param name="model">參採情形 Model</param>
        /// <returns></returns>
        public async Task UpdateRDResSituation(ResSituationModel model)
        {
            string sql = $@"
                    update
                        RD_RES_SITUATION
                    set
                        SITUATION_STATUS = '1'
                       ,SITUATION_TYPE = @SITUATION_TYPE
                       ,SITUATION_DESC = @SITUATION_DESC
                       ,CLOSING_DATE = @CLOSING_DATE
                       ,MDF_USER = @MDF_USER
                       ,MDF_DATE = {DTNow}
                    where
                        PLAN_NO = @PLAN_NO";

            await ExecuteCommandAsync(sql.ToString(), model, RDDBKey);
        }

        /// <summary>
        /// 儲存續列管一年內參採情形
        /// </summary>
        /// <param name="model">參採情形 Model</param>
        /// <returns></returns>
        public async Task<bool> UpdateRDResSituaContinue(ResSituationModel model)
        {
            string sql = $@"
                    update
                        RD_RES_SITUACONTINUE
                    set
                         SITUACONTINUE_STATUS = '1'
                        ,SITUATION_TYPE = @CONTINUE_SITUAITON_TYPE
                        ,CONTINUE_SITUAITON_DESC = @CONTINUE_SITUAITON_DESC
                        ,MDF_USER = @MDF_USER
                        ,MDF_DATE = {DTNow}
                    where
                        PLAN_NO = @PLAN_NO";

            return await ExecuteCommandAsync(sql.ToString(), model, RDDBKey);
        }

        /// <summary>
        /// 建立參採情形/結案成果填報結果
        /// </summary>
        /// <param name="model">參採情形 Model</param>
        /// <returns></returns>
        public async Task InsertRDResSituation(ResSituationModel model)
        {
            string sql = $@"INSERT INTO RD_RES_SITUATION
                                (PLAN_NO
                                ,SITUATION_TYPE
                                ,SITUATION_DESC
                                ,SITUATION_STATUS
                                ,LOCK_YN
                                ,CLOSING_DATE
                                ,CRT_USER
                                ,CRT_DATE
                                ,MDF_USER
                                ,MDF_DATE)
                             VALUES
                                (@PLAN_NO
                                ,@SITUATION_TYPE
                                ,@SITUATION_DESC
                                ,'1'
                                ,'N'
                                ,@CLOSING_DATE
                                ,@CRT_USER
                                ,{DTNow}
                                ,@MDF_USER
                                ,{DTNow})";

            await ExecuteCommandAsync(sql.ToString(), model, RDDBKey);
        }

        /// <summary>
        /// 建立續列管一年內參採情形
        /// </summary>
        /// <param name="model">參採情形 Model</param>
        /// <returns></returns>
        public async Task InsertRDResSituaContinue(ResSituationModel model)
        {
            string sql = $@"INSERT INTO RD_RES_SITUACONTINUE
                                (PLAN_NO
                                ,SITUATION_TYPE
                                ,SITUACONTINUE_STATUS
                                ,CONTINUE_SITUAITON_DESC
                                ,LOCK_YN
                                ,CRT_USER
                                ,CRT_DATE
                                ,MDF_USER
                                ,MDF_DATE)
                             VALUES
                                (@PLAN_NO
                                ,@CONTINUE_SITUAITON_TYPE
                                ,'1'
                                ,@CONTINUE_SITUAITON_DESC
                                ,'N'
                                ,@CRT_USER
                                ,{DTNow}
                                ,@MDF_USER
                                ,{DTNow})";

            await ExecuteCommandAsync(sql.ToString(), model, RDDBKey);
        }
    }
}
