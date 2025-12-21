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
    /// 計畫基本資料
    /// </summary>
    public class ProjectBasicDac : Dac, IProjectBasicDac
    {
        private readonly IUserData user;

        public ProjectBasicDac(IConnectionControlCenter connectionControlCenter,
           IHttpContextAccessor httpContextAccessor,
           ISqlTrace trace,
           IUserProfile profile,
           IParameterAdaptor ParameterAdaptor,
           IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
            user = profile.GetLoginUser();
        }

        /// <summary>
        /// 變更委託研究刪除與撤銷
        /// 執行類別 D 刪除 R 撤銷 L1 送出鎖定L2 解除鎖定
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> SaveRDBasicStatus(ProjectBasicStatusModel model)
        {
            StringBuilder sql = new();

            sql.AppendLine($@"update
                                RD_RESEARCH_BASIC
                              set 
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}, ");

            switch (model.EXEC_KIND)
            {
                // 刪除
                case "D":
                    sql.AppendLine(" REVOKED_YN = 'D'");
                    break;

                // 撤銷
                case "R":
                    sql.AppendLine(" REVOKED_YN = 'Y'");
                    break;

                // 送出鎖定
                case "L1":
                    sql.AppendLine(" LOCK_YN = 'Y'");
                    break;

                // 解除鎖定
                case "L2":
                    sql.AppendLine(" LOCK_YN = 'N'");
                    break;
                default:
                    break;
            }

            sql.AppendLine(" where PLAN_NO = @PLAN_NO");

            return await ExecuteCommandAsync(sql.ToString(),
                model.PLAN_NOS.Select(x => new { PLAN_NO = x, MDF_USER = UserId }), RDDBKey);
        }

        /// <summary>
        /// 取得委託研究的基本資料
        /// </summary>
        /// <param name="PLAN_NO">計畫編號</param>
        /// <returns></returns>
        public async Task<ResearchBasicModel> GetRDResearchBasic(string PLAN_NO)
        {
            string sql = @"select
	                            PLAN_YEAR,
	                            PLAN_ID,
	                            PLAN_NO,
	                            RESEARCH_STATUS,
	                            PLAN_NAME,
	                            ENTRUST_UNIT_NAME,
	                            RESEARCH_NAME,
	                            PLAN_START_DATE,
	                            PLAN_END_DATE,
	                            MID_REPORT_YM,
	                            FINAL_REPORT_YM,
	                            PUBLIC_MONEY,
	                            FUND_MONEY,
	                            CENTER_MONEY,
	                            OTHER_MONEY,
	                            OTHER_DESC,
	                            PLAN_CAUSE,
	                            PLAN_CONTENT,
	                            PLAN_EXPECTED,
	                            CONTACT_NAME,
	                            CONTACT_TEL,
	                            CONTACT_EMAIL,
	                            ASSIGNE_EMAIL,
                                OU_ID,
	                            dbo.FN_GetOuName(OU_ID, 3) as EXEC_ORG_NAME,
	                            LOCK_YN
                             from RD_RESEARCH_BASIC(nolock)
                             where PLAN_NO = @PLAN_NO ";

            return await ExecuteQueryFirstOrDefaultAsync<ResearchBasicModel>(sql, new { PLAN_NO } , RDDBKey);
        }

        /// <summary>
        /// 取得委託研究的基本資料的評核指標資料
        /// （流水號、評核類別、評核項目、預期完成日期）
        /// </summary>
        /// <param name="planNo">計畫編號</param>
        /// <returns></returns>
        public async Task<List<PolicyIndexModel>> GetPolicyIndexData(string PLAN_NO)
        {
            string sql = @"
                        select
	                        M1.SEQ,
	                        M1.POLICY_KIND,
	                        M1.POLICY_INDEX_DESC,
	                        M1.RES_FINISH_DATE
                        from
                            RD_RES_POLICY_INDEX M1(nolock)
                        where M1.PLAN_NO = @PLAN_NO";
                
            return (await ExecuteQueryAsync<PolicyIndexModel>(sql, new { PLAN_NO }, RDDBKey)).ToList();
        }


        /// <summary>
        /// 儲存委託研究基本資料，不包含評核指標資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> UpdateRDResearchBasic(ResearchBasicModel model)
        {
            string sql = $@"UPDATE
                                RD_RESEARCH_BASIC
                            SET
                                PLAN_YEAR = @PLAN_YEAR,
                                PLAN_NAME = @PLAN_NAME,
                                ENTRUST_UNIT_NAME = @ENTRUST_UNIT_NAME,
                                RESEARCH_NAME = @RESEARCH_NAME,
                                PLAN_START_DATE = @PLAN_START_DATE,
                                PLAN_END_DATE = @PLAN_END_DATE,
                                PUBLIC_MONEY = @PUBLIC_MONEY,
                                FUND_MONEY = @FUND_MONEY,
                                CENTER_MONEY = @CENTER_MONEY,
                                OTHER_MONEY = @OTHER_MONEY,
                                OTHER_DESC = @OTHER_DESC,
                                PLAN_CAUSE = @PLAN_CAUSE,
                                PLAN_CONTENT = @PLAN_CONTENT,
                                PLAN_EXPECTED = @PLAN_EXPECTED,
                                CONTACT_NAME = @CONTACT_NAME,
                                CONTACT_TEL = @CONTACT_TEL,
                                CONTACT_EMAIL = @CONTACT_EMAIL,
                                ASSIGNE_EMAIL = @ASSIGNE_EMAIL,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                             WHERE
                                PLAN_ID = @PLAN_ID";

            return await ExecuteCommandAsync(sql, model, RDDBKey);
        }

        /// <summary>
        /// 新增 評核指標資料
        /// </summary>
        /// <param name="model"></param>
        /// <remarks> STATUS 狀態: 1:未送審、2:送審、3:審核退回、4:審核通過</remarks>
        public async Task InsertPolicyIndex(List<PolicyIndexModel> model)
        {
            string sql = $@"INSERT INTO RD_RES_POLICY_INDEX
                                (PLAN_ID
                                ,PLAN_NO
                                ,POLICY_INDEX_DESC
                                ,POLICY_KIND
                                ,RES_FINISH_DATE
                                ,STATUS
                                ,CRT_USER
                                ,CRT_DATE
                                ,MDF_USER
                                ,MDF_DATE)
                             VALUES
                                (@PLAN_ID
                                ,@PLAN_NO
                                ,@POLICY_INDEX_DESC
                                ,@POLICY_KIND
                                ,@RES_FINISH_DATE
                                ,'1'
                                ,@CRT_USER
                                ,{DTNow}
                                ,@MDF_USER
                                ,{DTNow})";
            
            await ExecuteCommandAsync(sql, model, RDDBKey);
        }

        /// <summary>
        /// 修改 評核指標資料
        /// </summary>
        /// <param name="model"></param>
        public async Task UpdatePolicyIndex(List<PolicyIndexModel> model)
        {
            string sql = $@"UPDATE RD_RES_POLICY_INDEX
                            SET
                                  POLICY_INDEX_DESC = @POLICY_INDEX_DESC
                                 ,RES_FINISH_DATE = @RES_FINISH_DATE
                                 ,POLICY_KIND = @POLICY_KIND
                                 ,MDF_USER = @MDF_USER
                                 ,MDF_DATE = {DTNow}
                            WHERE
                                  SEQ = @SEQ";
            await ExecuteCommandAsync(sql, model, RDDBKey);
        }

        /// <summary>
        /// 刪除 評核指標資料
        /// </summary>
        /// <param name="seqs"></param>
        public async Task DeletePolicyIndex(List<int> seqs)
        {
            string sql = @" DELETE FROM RD_RES_POLICY_INDEX
                           WHERE SEQ in @seqs";

            await ExecuteCommandAsync(sql, new { seqs }, RDDBKey);
        }

        /// <summary>
        /// 新增委託研究基本資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks> RESEARCH_STATUS 狀態: 1:未送審、2:送審、3:審核退回、4:審核通過</remarks>
        public async Task InsertRDResearchBasic(ResearchBasicModel model)
        {
            string sql = $@"INSERT INTO
                                RD_RESEARCH_BASIC
                                (PLAN_NO
                                ,PLAN_NAME
                                ,RESEARCH_STATUS
                                ,OU_ID
                                ,PLAN_YEAR
                                ,LOCK_YN
                                ,OPEN_EXTENSION_YN
                                ,REVOKED_YN
                                ,CRT_USER
                                ,CRT_DATE
                                ,MDF_USER
                                ,MDF_DATE)
                             VALUES
                                (@PLAN_NO
                                ,@PLAN_NAME
                                ,'1'
                                ,@OU_ID
                                ,@PLAN_YEAR
                                ,'N'
                                ,'N'
                                ,'N'
                                ,@CRT_USER
                                ,{DTNow}
                                ,@MDF_USER
                                ,{DTNow})";

            await ExecuteCommandAsync(sql, model, RDDBKey);
        }

        /// <summary>
        /// 取得計畫流水號
        /// 年度(3碼)+流水號(5碼)例：10400001
        /// </summary>
        /// <param name="planYear">計畫年度</param>
        /// <returns></returns>
        /// <remark>找年度開頭的計畫編號最大值加一當作新計畫編號流水號，若找不到則回傳0</remark>
        public string GetPlanNoSeq(string planYear)
        {
            string sql = @"SELECT
                                ISNULL(MAX(CAST(RIGHT(PLAN_NO,5) AS INT)),0)+1 AS NUM 
                           FROM
                                RD_RESEARCH_BASIC (noLock)
                           WHERE
                                PLAN_NO
                           LIKE 
                                @planYear + '%'";
            return ExecuteQuery<string>(sql, new { planYear }, RDDBKey).FirstOrDefault();
        }
    }
}
