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
    /// 展延紀錄
    /// </summary>
    public class ProjectExtensionDac : Dac, IProjectExtensionDac
    {
        private readonly IUserData user;
        public ProjectExtensionDac(
          IConnectionControlCenter connectionControlCenter,
          IHttpContextAccessor httpContextAccessor,
          ISqlTrace trace,
          IUserProfile profile,
          IParameterAdaptor ParameterAdaptor,
          IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
            user = profile.GetLoginUser();
        }

        /// <summary>
        /// 取得展延紀錄清單
        /// </summary>
        /// <param name="PLAN_NO">計畫編號</param>
        /// <returns></returns>
        public async Task<List<ProjectExtensionModel>> GetRDExtensionList(string PLAN_NO)
        {
            string sql = @$"
                        select
	                        M1.EXTENSION_NO,
	                        M1.PLAN_START_DATE,
	                        M1.PLAN_END_DATE,
	                        M1.EXTP_LANEND_DATE,
	                        M1.CRT_DATE as APPLY_DATE,
	                        scUser.USR_NAME as APPLICANT,
	                        dbo.FN_GetSetParam('EXTENSION_STATUS', M1.EXTENSION_STATUS) as EXTENSION_STATUS,
	                        M1.EXTENSION_STATUS as EXTENSION_STATUS_CODE,
	                        M2.RD_RES_POLICY_INDEX_COUNT,
                            COALESCE(M3.STATUS_3_COUNT, 0) AS STATUS_3_COUNT
                         from
	                         RD_RES_EXTENSION M1(nolock)
                         left join
                             {SC30_M}.SCUSERM scUser(nolock)
                         on
                             M1.CRT_USER = scUser.USR_ID
                        right join (
                            select
                                PLAN_NO,
                                COUNT(*) AS RD_RES_POLICY_INDEX_COUNT
                            from
                                RD_RES_POLICY_INDEX(nolock)
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
                                RD_RES_POLICY_INDEX(nolock)
                            where
                                PLAN_NO = @PLAN_NO
                            and
                                STATUS = '3'
                            group by
                                PLAN_NO
                        ) as M3
                        on
                            M2.PLAN_NO = M3.PLAN_NO
                         where 
	                         M2.PLAN_NO = @PLAN_NO";

            return (await ExecuteQueryAsync<ProjectExtensionModel>(sql, new { PLAN_NO }, RDDBKey)).ToList();
        }

        /// <summary>
        /// 取得展延紀錄明細
        /// </summary>
        /// <param name="EXTENSION_NO">展延編號</param>
        /// <returns></returns>
        public async Task<ProjectExtensionModel> GetRDExtension(string EXTENSION_NO)
        {
            string sql = @"
                        select
	                        EXTENSION_ID,
	                        EXTENSION_STATUS,
	                        PLAN_START_DATE,
	                        PLAN_END_DATE,
	                        PLAN_NO,
	                        EXT_REASON,
	                        EXTP_LANEND_DATE
                        from
	                        RD_RES_EXTENSION(nolock)
                        where 
	                        EXTENSION_NO = @EXTENSION_NO";

            return await ExecuteQueryFirstOrDefaultAsync<ProjectExtensionModel>(sql, new { EXTENSION_NO }, RDDBKey);
        }

        /// <summary>
        /// 儲存展延紀錄
        /// </summary>
        /// <param name="model">展延紀錄 Model</param>
        /// <returns></returns>
        /// <remarks>
        /// 展延送審狀態 EXTENSION_STATUS
        /// 1:未送審、2:待審核、3:審核通過、4:審核不通過、5:取消申請
        /// </remarks>
        public async Task SaveRDExtension(ProjectExtensionModel model)
        {
            string sql = $@"
                        UPDATE RD_RES_EXTENSION
                        SET EXTENSION_STATUS = '1'
                           ,EXTP_LANEND_DATE = @EXTP_LANEND_DATE
                           ,EXT_REASON = @EXT_REASON
                           ,MDF_USER = @MDF_USER
                           ,MDF_DATE = {DTNow}
                     WHERE EXTENSION_NO = @EXTENSION_NO";

            await ExecuteCommandAsync(sql, model, RDDBKey);
        }

        /// <summary>
        /// 建立展延紀錄(Main)
        /// </summary>
        /// <param name="model">展延紀錄 Model</param>
        /// <returns>回傳展延序號 EXTENSION_ID</returns>
        /// <remarks>
        /// 展延送審狀態 EXTENSION_STATUS
        /// 1:未送審、2:待審核、3:審核通過、4:審核不通過、5:取消申請
        /// </remarks>
        public async Task<int> InsertRDExtension(ProjectExtensionModel model)
        {
            string sql = $@"INSERT INTO
                                RD_RES_EXTENSION
                                (PLAN_NO
                                ,EXTENSION_NO
                                ,EXTENSION_STATUS
                                ,PLAN_START_DATE
                                ,PLAN_END_DATE
                                ,CRT_USER
                                ,CRT_DATE
                                ,MDF_USER
                                ,MDF_DATE)
                             SELECT
                                 @PLAN_NO
                                ,@EXTENSION_NO
                                ,'5'
                                ,PLAN_START_DATE
                                ,PLAN_END_DATE
                                ,@MDF_USER
                                ,{DTNow}
                                ,@MDF_USER
                                ,{DTNow}
                             FROM
                                RD_RESEARCH_BASIC M1(NOLOCK)
                             WHERE
                                M1.PLAN_NO = @PLAN_NO
                              SELECT SCOPE_IDENTITY()";

            return (await ExecuteQueryAsync<int>(sql, model, RDDBKey)).FirstOrDefault();
        }

        /// <summary>
        /// 展延紀錄評核指標 轉檔 RD_RES_POLICY_INDEX => RD_RES_POLICY_INDEX_ADJ
        /// </summary>
        /// <param name="policySEQs">展延紀錄序號</param>
        /// <param name="EXTENSION_NO">展延編號</param>
        /// <returns></returns>
        public async Task TransferRDExtensionPolicyIndexAdj(List<int> policySEQs, string EXTENSION_NO, int EXTENSION_ID)
        {
            string sql = @"
                        INSERT INTO RD_RES_POLICY_INDEX_ADJ
                                (SEQ
	                            ,EXTENSION_NO
	                            ,EXTENSION_ID
	                            ,PLAN_ID
	                            ,PLAN_YEAR
	                            ,SEASON_TYPE
	                            ,PLAN_NO
	                            ,POLICY_INDEX_DESC
	                            ,EXECUTION_DESC
	                            ,PROGRESS_TYPE
	                            ,BEHIND_REASON
	                            ,SOLUTIONS
	                            ,MID_FINISH_DATE
	                            ,FINAL_FINISH_DATE
	                            ,MID_ATTACHMENT_ID
	                            ,MID_ATTACHMENT_NAME
	                            ,FINAL_ATTACHMENT_ID
	                            ,FINAL_ATTACHMENT_NAME
	                            ,AWARD_DATE
	                            ,AWARD_ATTACHMENT_ID
	                            ,AWARD_ATTACHMENT_NAME
	                            ,STATUS
	                            ,POLICY_KIND
	                            ,RES_FINISH_DATE)
                        SELECT 
	                         SEQ
	                        ,@EXTENSION_NO
	                        ,@EXTENSION_ID
	                        ,PLAN_ID
	                        ,PLAN_YEAR
	                        ,SEASON_TYPE
	                        ,PLAN_NO
	                        ,POLICY_INDEX_DESC
	                        ,EXECUTION_DESC
	                        ,PROGRESS_TYPE
	                        ,BEHIND_REASON
	                        ,SOLUTIONS
	                        ,MID_FINISH_DATE
	                        ,FINAL_FINISH_DATE
	                        ,MID_ATTACHMENT_ID
	                        ,MID_ATTACHMENT_NAME
	                        ,FINAL_ATTACHMENT_ID
	                        ,FINAL_ATTACHMENT_NAME
	                        ,AWARD_DATE
	                        ,AWARD_ATTACHMENT_ID
	                        ,AWARD_ATTACHMENT_NAME
	                        ,STATUS
	                        ,POLICY_KIND
	                        ,RES_FINISH_DATE
                        FROM RD_RES_POLICY_INDEX(nolock)
                        WHERE RD_RES_POLICY_INDEX.SEQ in @policySEQs ";

            await ExecuteCommandAsync(sql, new { policySEQs, EXTENSION_NO, EXTENSION_ID }, RDDBKey);
        }

        
        /// <summary>
        /// 取得展延紀錄明細的評核指標資料
        /// （流水號、評核類別、評核項目、預期完成期程、調整預期完成日期）
        /// </summary>
        /// <param name="PLAN_NO">計畫編號</param>
        /// <returns></returns>
        public async Task<List<ExtensionPolicyIndexModel>> GetPolicyIndexData(string PLAN_NO)
        {
            string sql = @"
                        select
	                        SEQ,
	                        POLICY_KIND,
	                        POLICY_INDEX_DESC,
	                        RES_FINISH_DATE
                        from
	                        RD_RES_POLICY_INDEX(nolock)
                        where
                            PLAN_NO = @PLAN_NO";

            return (await ExecuteQueryAsync<ExtensionPolicyIndexModel>(sql, new { PLAN_NO }, RDDBKey)).ToList();
        }

        /// <summary>
        /// 取得評核指標調整表(ADJ)所需的評核指標資料
        /// </summary>
        /// <param name="EXTENSION_NO">展延編號</param>
        /// <returns></returns>
        /// <remarks>
        /// ˙EDIT_STATUS 是 D 代表在調整表已刪除此評核指標資料
        /// ˙轉移(Transfer) 到調整表的資料，最一開始 EDIT_STATUS 會是 NULL
        /// </remarks>
        public async Task<List<ExtensionPolicyIndexModel>> GetExtensionPolicyIndexData(string EXTENSION_NO)
        {
            string sql = @"
                        select
	                        POLICY_INDEX_ADJ_ID,
	                        PLAN_ID,
	                        PLAN_NO,
	                        POLICY_INDEX_DESC,
	                        EXECUTION_DESC,
	                        SEQ,
	                        POLICY_KIND,
	                        RES_FINISH_DATE,
	                        POLICY_EXTP_LANEND_DATE
                        from
                            RD_RES_POLICY_INDEX_ADJ(nolock)
                        where
                            EXTENSION_NO = @EXTENSION_NO
                        and
                            (EDIT_STATUS <> 'D'or EDIT_STATUS IS NULL)";

            return (await ExecuteQueryAsync<ExtensionPolicyIndexModel>(sql, new { EXTENSION_NO }, RDDBKey)).ToList();
        }

        /// <summary>
        /// 新增 評核指標調整表的資料
        /// </summary>
        /// <param name="model"></param>
        /// <remarks>
        /// STATUS 狀態: 1:未送審、2:送審、3:審核退回、4:審核通過
        /// EDIT_STATUS 編輯狀態: A:新增、E：編輯、D：刪除
        /// </remarks>
        public async Task InsertPolicyIndex(List<ExtensionPolicyIndexModel> model)
        {
            string sql = $@"INSERT INTO RD_RES_POLICY_INDEX_ADJ
                                (EXTENSION_NO
                                ,EXTENSION_ID
                                ,PLAN_ID
                                ,PLAN_NO
                                ,POLICY_INDEX_DESC
                                ,POLICY_KIND
                                ,RES_FINISH_DATE
                                ,POLICY_EXTP_LANEND_DATE
                                ,EXTP_LANEND_DATE
                                ,EDIT_STATUS
                                ,STATUS
                                ,CRT_USER
                                ,CRT_DATE
                                ,MDF_USER
                                ,MDF_DATE)
                             VALUES
                                (@EXTENSION_NO
                                ,@EXTENSION_ID
                                ,@PLAN_ID
                                ,@PLAN_NO
                                ,@POLICY_INDEX_DESC
                                ,@POLICY_KIND
                                ,@RES_FINISH_DATE
                                ,@POLICY_EXTP_LANEND_DATE
                                ,@RES_FINISH_DATE
                                ,'A'
                                ,'1'
                                ,@CRT_USER
                                ,{DTNow}
                                ,@MDF_USER
                                ,{DTNow})";
            await ExecuteCommandAsync(sql, model, RDDBKey);
        }

        /// <summary>
        /// 修改 評核指標調整表的資料
        /// </summary>
        /// <param name="model"></param>
        public async Task UpdatePolicyIndex(List<ExtensionPolicyIndexModel> model)
        {
            string sql = $@"UPDATE RD_RES_POLICY_INDEX_ADJ
                            SET
                                  POLICY_INDEX_DESC = @POLICY_INDEX_DESC
                                 ,POLICY_KIND = @POLICY_KIND
                                 ,RES_FINISH_DATE = @RES_FINISH_DATE
                                 ,EXTP_LANEND_DATE = @RES_FINISH_DATE
                                 ,POLICY_EXTP_LANEND_DATE = @POLICY_EXTP_LANEND_DATE
                                 ,EDIT_STATUS = 'E'
                                 ,MDF_USER = @MDF_USER
                                 ,MDF_DATE = {DTNow}
                            WHERE
                                  POLICY_INDEX_ADJ_ID = @POLICY_INDEX_ADJ_ID";
            await ExecuteCommandAsync(sql, model, RDDBKey);
        }

        /// <summary>
        /// 刪除 評核指標調整表的資料
        /// </summary>
        /// <param name="model"></param>
        public async Task DeletePolicyIndex(List<ExtensionPolicyIndexModel> model)
        {
            string sql = $@"UPDATE RD_RES_POLICY_INDEX_ADJ
                            SET  EDIT_STATUS = 'D'
                                ,MDF_USER = @MDF_USER
                                ,MDF_DATE = {DTNow}
                            WHERE
                                  POLICY_INDEX_ADJ_ID = @POLICY_INDEX_ADJ_ID";
            await ExecuteCommandAsync(sql, model, RDDBKey);
        }

        /// <summary>
        /// 取得展延編號流水號
        /// 年度(3碼)+流水號(5碼)例：111300001
        /// </summary>
        /// <param name="planYear">計畫年度</param>
        /// <returns></returns>
        /// <remark>找年度開頭的展延編號最大值加一當作新計畫編號流水號，若找不到則回傳0</remark>
        public string GetExtensionNoSeq(string planYear)
        {
            string sql = @"SELECT
                                ISNULL(MAX(CAST(RIGHT(EXTENSION_NO,5) AS INT)),0)+1 AS NUM 
                           FROM
                                RD_RES_EXTENSION (noLock)
                           WHERE
                                EXTENSION_NO
                           LIKE 
                                @planYear + '%'";
            return ExecuteQuery<string>(sql, new { planYear }, RDDBKey).FirstOrDefault();
        }
    }
}
