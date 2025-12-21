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
    public class RDProjectAuditService : Dac, IRDProjectAuditDac
    {
        private readonly IUserData user;
        public RDProjectAuditService(
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
        /// 取得審核紀錄清單
        /// </summary>
        /// <param name="model">審查資料檔 Model</param>
        /// <returns></returns>
        public async Task<List<RDAuditModel>> GetRDAuditList(string MAIN_NO)
        {
            string sql = $@"
                    SELECT
                       AUDIT_ID
                      ,MAIN_NO
                      ,SUB_NO
                      ,PLAN_REVIEW_TYPE
                      ,dbo.FN_GetSetParam('PLAN_REVIEW_TYPE', PLAN_REVIEW_TYPE) as PLAN_REVIEW
                      ,REVIEW_RESULT
                      ,dbo.FN_GetSetParam('REVIEW_RESULT_STATUS', REVIEW_RESULT) as REVIEW_RESULT_STATUS
                      ,REVIEW_COMMENTS
                      ,IS_SEND
                  FROM
                       RD_AUDIT M1(nolock)
                  WHERE
                       MAIN_NO = @MAIN_NO
                  ORDER BY M1.AUDIT_ID";

            return (await ExecuteQueryAsync<RDAuditModel>(sql, new { MAIN_NO }, RDDBKey)).ToList();
        }

        /// <summary>
        /// 建立審核紀錄
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>SUB_NO只有在執行情形填報的評核指標跟展延申請的送審才有值</remarks>
        public async Task InsertRDAudit(RDAuditModel model)
        {
            string sql = $@"
                    INSERT INTO RD_AUDIT
                        (AUDIT_ID
                        ,MAIN_NO
                        ,SUB_NO
                        ,PLAN_REVIEW_TYPE
                        ,CRT_USER
                        ,CRT_DATE
                        ,MDF_USER
                        ,MDF_DATE)
                    VALUES
                        (@AUDIT_ID
                        ,@MAIN_NO
                        ,@SUB_NO
                        ,@PLAN_REVIEW_TYPE
                        ,@CRT_USER
                        ,{DTNow}
                        ,@MDF_USER
                        ,{DTNow})";

            await ExecuteCommandAsync(sql.ToString(), model, RDDBKey);
        }

        /// <summary>
        /// 編輯審核紀錄
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// 【存檔】不改 IS_SEND，【確認送出】改 IS_SEND 為 1
        /// </remarks>
        public async Task UpdateRDAudit(RDAuditModel model)
        {
            StringBuilder sql = new ();
            sql.AppendLine($@"
                    UPDATE RD_AUDIT
                    SET 
                        REVIEW_RESULT = @REVIEW_RESULT
                       ,REVIEW_COMMENTS = @REVIEW_COMMENTS
                       ,MDF_USER = @MDF_USER
                       ,MDF_DATE = {DTNow}");

            // IS_SEND == 1，確認送出
            if (model.IS_SEND == 1)
            {
                sql.AppendLine(" ,IS_SEND = @IS_SEND");
            }

            sql.AppendLine(" WHERE AUDIT_ID = @AUDIT_ID");
            await ExecuteCommandAsync(sql.ToString(), model, RDDBKey);
        }

        /// <summary>
        /// 更改審核狀態
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task ChangReviewStatus(RDAuditStatusModel model)
        {
            StringBuilder sql = new();
            sql.AppendLine($@"
                        UPDATE {model.TABLE}
                        SET {model.STATUS_FIELD} = @STATUS
                           ,MDF_USER = @MDF_USER
                           ,MDF_DATE = {DTNow}
                        WHERE
                            PLAN_NO = @MAIN_NO");
            if(model.SUB_NO != 0)
            {
                sql.AppendLine($" AND {model.ID_FIELD} = @SUB_NO");
            }

            await ExecuteCommandAsync(sql.ToString(), model, RDDBKey);
        }

        /// <summary>
        /// 透過展延序號撈取評核指標調整表資料
        /// </summary>
        /// <param name="EXTENSION_ID">展延序號</param>
        /// <returns></returns>
        public async Task<List<ExtensionPolicyIndexModel>> GetExtensionPolicyIndexDataById(int EXTENSION_ID)
        {
            string sql = @"
                        select
	                        SEQ,
	                        EDIT_STATUS
                        from
                            RD_RES_POLICY_INDEX_ADJ(nolock)
                        where
                            EXTENSION_ID = @EXTENSION_ID";

            return (await ExecuteQueryAsync<ExtensionPolicyIndexModel>(sql, new { EXTENSION_ID }, RDDBKey)).ToList();
        }

        /// <summary>
        /// 展延紀錄評核指標 轉檔 編輯 RD_RES_POLICY_INDEX_ADJ => RD_RES_POLICY_INDEX
        /// </summary>
        /// <param name="SEQ">評核指標序號</param>
        /// <param name="EXTENSION_ID">展延序號</param>
        /// <returns></returns>
        public async Task TransferRDExtensionPolicyIndexUpdate(int SEQ, int EXTENSION_ID)
        {
            string sql = $@"
                        UPDATE M1
						SET
                             POLICY_KIND = M2.POLICY_KIND
							,POLICY_INDEX_DESC = M2.POLICY_INDEX_DESC
							,RES_FINISH_DATE = M2.RES_FINISH_DATE
							,MDF_USER = @MDF_USER
							,MDF_DATE = {DTNow}
                        FROM
                             RD_RES_POLICY_INDEX M1
						INNER JOIN
                            (SELECT
                                SEQ,
                                EXTENSION_ID,
                                POLICY_KIND,
                                POLICY_INDEX_DESC,
                                RES_FINISH_DATE
                             FROM
                                RD_RES_POLICY_INDEX_ADJ M2(nolock)) M2
                         ON
                             M1.SEQ = M2.SEQ
						 WHERE
                              M1.SEQ = @SEQ
                         AND
                              M2.EXTENSION_ID = @EXTENSION_ID";

            await ExecuteCommandAsync(sql, new { SEQ, EXTENSION_ID, MDF_USER = user.USER_NAME }, RDDBKey);
        }

        /// <summary>
        /// 展延紀錄評核指標 轉檔 刪除 RD_RES_POLICY_INDEX_ADJ => RD_RES_POLICY_INDEX
        /// </summary>
        /// <param name="SEQ">評核指標序號</param>
        /// <returns></returns>
        public async Task TransferRDExtensionPolicyIndexDelete(int SEQ)
        {
            string sql = @"
                        DELETE
                            M1
                        FROM
                            RD_RES_POLICY_INDEX M1
                        INNER JOIN
                            RD_RES_POLICY_INDEX_ADJ M2
                        ON
                            M1.SEQ = M2.SEQ
                        WHERE
                            M2.EDIT_STATUS = 'D'
                        AND
                            M1.SEQ = @SEQ";

            await ExecuteCommandAsync(sql, new { SEQ }, RDDBKey);
        }

        /// <summary>
        /// 展延紀錄評核指標 轉檔 新增 RD_RES_POLICY_INDEX_ADJ => RD_RES_POLICY_INDEX
        /// </summary>
        /// <param name="EXTENSION_ID">展延序號</param>
        /// <returns></returns>
        public async Task TransferRDExtensionPolicyIndexInsert(int EXTENSION_ID)
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
                             SELECT
                                 PLAN_ID
                                ,PLAN_NO
                                ,POLICY_INDEX_DESC
                                ,POLICY_KIND
                                ,RES_FINISH_DATE
                                ,'3'
							    ,@USER
							    ,{DTNow}
                                ,@USER
                                ,{DTNow}
                              FROM
                                 RD_RES_POLICY_INDEX_ADJ M2
                              WHERE
                                 M2.EXTENSION_ID = @EXTENSION_ID
                              AND
                                 M2.SEQ IS NULL";

            await ExecuteCommandAsync(sql, new { EXTENSION_ID, USER = user.USER_NAME }, RDDBKey);
        }

        /// <summary>
        /// 產生審查紀錄編號
        /// planReviewType + 民國年度(3碼) + 流水號(4碼)例：A1113010001
        /// </summary>
        /// <param name="planYear">送審年度</param>
        /// <param name="planMonth">送審月份</param>
        /// <param name="planReviewType">審核類別</param>
        /// <returns></returns>
        /// <remark>
        /// 找審核類別+年度+流水號開頭的審查紀錄編號最大值加一當作新審查紀錄編號流水號
        /// 若找不到則回傳0
        /// </remark>
        public string GetRDAuditIdSeq(string planYear, string planMonth, string planReviewType)
        {
            string sql = @"SELECT
                                ISNULL(MAX(CAST(RIGHT(AUDIT_ID,4) AS INT)),0)+1 AS NUM 
                           FROM
                                RD_AUDIT (noLock)
                           WHERE
                                AUDIT_ID
                           LIKE 
                                @planReviewType + @planYear + @planMonth + '%'";
            return ExecuteQuery<string>(sql, new { planYear, planMonth, planReviewType }, RDDBKey).FirstOrDefault();
        }
    }
}
