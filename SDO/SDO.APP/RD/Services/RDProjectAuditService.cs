using SDO.Models;
using SDO.Dac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDO.Base.Utils;
using SDO.Utils;
using System.Transactions;

namespace SDO.Services
{
    public class RDProjectAuditService : Service, IRDProjectAuditService
    {
        private readonly IRDProjectAuditDac dac;
        public RDProjectAuditService
        (
            IRDProjectAuditDac dac
        )
        {
            this.dac = dac;
        }

        /// <summary>
        /// 取得審核紀錄清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<RDAuditModel>> GetRDAuditList(string MAIN_NO)
        {
            return await dac.GetRDAuditList(MAIN_NO);
        }

        /// <summary>
        /// 儲存審核紀錄
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task SaveRDAudit(RDAuditModel model)
        {
            // 審核通過
            string STATUS = "3";
            //bool isSaveData = false;

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // 編輯審查紀錄
                await dac.UpdateRDAudit(model);
                // 1 是送審，0 是存檔
                if (model.IS_SEND == 1)
                {
                    // 前端審查結果: 通過(Y)
                    // 展延申請(A5)，要寫回去主表 RD_RES_POLICY_INDEX_ADJ => RD_RES_POLICY_INDEX
                    if (model.REVIEW_RESULT == "Y" && model.PLAN_REVIEW_TYPE == "A5")
                    {
                        await ApproveRDExtension(model.MAIN_NO, model.SUB_NO);
                    }
                    // 前端審查結果: 不通過(N)
                    else if (model.REVIEW_RESULT == "N")
                    {
                        // 審核退回
                        STATUS = "4";
                    }
                    // 審核狀態 Model
                    RDAuditStatusModel rdAuditStatusModel = new()
                    {
                        PLAN_REVIEW_TYPE = model.PLAN_REVIEW_TYPE,
                        MAIN_NO = model.MAIN_NO,
                        SUB_NO = model.SUB_NO,
                        STATUS = STATUS, // 審核狀態
                    };
                    // 更改各章節審核狀態
                    await ChangeReviewStatus(rdAuditStatusModel);
                }
                scope.Complete();
            }
        }

        /// <summary>
        /// 更改審核狀態，以及審核前要儲存對應的章節資料
        /// </summary>
        /// <param name="model"></param>
        /// <param name="isSaveData"></param>
        /// <returns></returns>
        public async Task ChangeReviewStatus(RDAuditStatusModel model)
        {
            // 各章節章節資料表名
            string table = "";
            // 資料表的審核狀態欄位名
            string field = "";
            // 資料表的序號(SEQ 或 EXTENSION_ID)欄位名
            string id = "";
            switch (model.PLAN_REVIEW_TYPE)
            {
                // 基本資料
                case "A1":
                    table = "RD_RESEARCH_BASIC";
                    field = "RESEARCH_STATUS";
                    break;
                // 執行情形填報
                case "A2":
                    table = "RD_RES_POLICY_INDEX";
                    field = "STATUS";
                    id = "SEQ";
                    break;
                // 結案成果填報
                case "A3":
                    table = "RD_RES_SITUATION";
                    field = "SITUATION_STATUS";
                    break;
                // 續列管一年內參採情形
                case "A4":
                    table = "RD_RES_SITUACONTINUE";
                    field = "SITUACONTINUE_STATUS";
                    break;
                // 展延申請
                case "A5":
                    table = "RD_RES_EXTENSION";
                    field = "EXTENSION_STATUS";
                    id = "EXTENSION_ID";
                    break;
                default:
                    break;
            }
            model.TABLE = table;
            model.STATUS_FIELD = field;
            model.ID_FIELD = id;
            await dac.ChangReviewStatus(model);
        }

        /// <summary>
        /// 展延紀錄審核通過，將評核指標調整表寫回評核指標資料表 RD_RES_POLICY_INDEX_ADJ => RD_RES_POLICY_INDEX
        /// </summary>
        /// <param name="mainNo">計畫編號</param>
        /// <param name="subNo">序號（展延序號 EXTENSION_ID）</param>
        /// <returns></returns>
        private async Task ApproveRDExtension(string mainNo, int subNo)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // 透過展延序號撈取評核指標調整表資料
                List<ExtensionPolicyIndexModel> policyIndexAdj = await dac.GetExtensionPolicyIndexDataById(subNo);
                foreach (ExtensionPolicyIndexModel item in policyIndexAdj)
                {
                    // 如果調整表資料沒有 SEQ，代表是新增
                    if (item.SEQ == 0)
                    {
                        await dac.TransferRDExtensionPolicyIndexInsert(subNo);
                    }
                    // 如果調整表的 EDIT_STATUS 是 D，代表是刪除
                    else if (item.EDIT_STATUS == "D")
                    {
                        await dac.TransferRDExtensionPolicyIndexDelete(item.SEQ);
                    }
                    // 如果調整表的 EDIT_STATUS 是 E，代表是編輯
                    else if (item.EDIT_STATUS == "E")
                    {
                        await dac.TransferRDExtensionPolicyIndexUpdate(item.SEQ, subNo);
                    }
                }
                scope.Complete();
            }
        }

        /// <summary>
        /// 產生審查紀錄編號
        /// planReviewType + 民國年度(3碼) + 流水號(4碼)例：A1113010001
        /// </summary>
        /// <param name="planYear">送審年度</param>
        /// <param name="planMonth">送審月份</param>
        /// <param name="planReviewType">審核類別</param>
        /// <returns></returns>
        public string GenRDAuditId(string planYear, string planMonth, string planReviewType)
        {
            // 取得流水號
            string seq = dac.GetRDAuditIdSeq(planYear, planMonth, planReviewType);
            return $"{planReviewType}{planYear}{planMonth}{seq.PadLeft(4, '0')}";
        }
    }
}
