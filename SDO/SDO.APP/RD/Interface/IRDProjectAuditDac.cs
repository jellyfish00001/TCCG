using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SDO.Dac
{
    public interface IRDProjectAuditDac : IDac
    {
        /// <summary>
        /// 取得審核紀錄清單
        /// </summary>
        /// <param name="model">審查資料檔 Model</param>
        /// <returns></returns>
        Task<List<RDAuditModel>> GetRDAuditList(string MAIN_NO);

        /// <summary>
        /// 建立審核紀錄
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task InsertRDAudit(RDAuditModel model);

        /// <summary>
        /// 編輯審核紀錄
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task UpdateRDAudit(RDAuditModel model);

        /// <summary>
        /// 更改審核狀態
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task ChangReviewStatus(RDAuditStatusModel model);

        /// <summary>
        /// 透過展延序號撈取評核指標調整表資料
        /// </summary>
        /// <param name="EXTENSION_ID">展延序號</param>
        /// <returns></returns>
        Task<List<ExtensionPolicyIndexModel>> GetExtensionPolicyIndexDataById(int EXTENSION_ID);

        /// <summary>
        /// 展延紀錄評核指標 轉檔 RD_RES_POLICY_INDEX_ADJ => RD_RES_POLICY_INDEX
        /// </summary>
        /// <param name="policySEQs">展延紀錄序號</param>
        /// <param name="EXTENSION_NO">展延編號</param>
        Task TransferRDExtensionPolicyIndexUpdate(int EXTENSION_ID, int SEQ);

        /// <summary>
        /// 展延紀錄評核指標 轉檔 刪除 RD_RES_POLICY_INDEX_ADJ => RD_RES_POLICY_INDEX
        /// </summary>
        /// <param name="SEQ">評核指標序號</param>
        /// <returns></returns>
        Task TransferRDExtensionPolicyIndexDelete(int SEQ);

        /// <summary>
        /// 展延紀錄評核指標 轉檔 新增 RD_RES_POLICY_INDEX_ADJ => RD_RES_POLICY_INDEX
        /// </summary>
        /// <param name="EXTENSION_ID">展延序號</param>
        /// <returns></returns>
        Task TransferRDExtensionPolicyIndexInsert(int EXTENSION_ID);

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
        string GetRDAuditIdSeq(string planYear, string planMonth, string planReviewType);
    }
}
