using SDO.APP.IPC.Models.ProjectAdjust;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 檢核點完成日期Model
    /// </summary>
    public class ProjectFillCkptComModel : DbEditor
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 執行類別
        /// </summary>
        public string CP_KIND { get; set; }

        /// <summary>
        /// 依契約預定完成日
        /// </summary>
        public DateTime? CONTRACT_FINISH_DATE { get; set; }

        /// <summary>
        /// 依契約預定完成日
        /// 因前端存檔用FormData包裝，所傳參數直接轉為String，故另定義字串型態參數接收
        /// </summary>
        public string CONTRACT_FINISH_DATE_FOR_SAVE { get; set; }

        /// <summary>
        /// 計畫實際聯絡人
        /// </summary>
        public string REAL_CONTACT { get; set; }

        /// <summary>
        /// 計畫實際聯絡人電話
        /// </summary>
        public string REAL_TEL { get; set; }

        /// <summary>
        /// 計畫實際聯絡人郵件
        /// </summary>
        public string REAL_EMAIL { get; set; }

        /// <summary>
        /// 使用國發會界接資料
        /// </summary>
        public bool? IS_USER_FTY_DATA { get; set; }

        /// <summary>
        /// (工程會)計畫UID
        /// </summary>
        public string PCC_PROJECT_UID { get; set; }

        /// <summary>
        /// (工程會)計畫編號
        /// </summary>
        public string PCC_PROJECT_NO { get; set; }

        /// <summary>
        /// (工程會)計畫名稱
        /// </summary>
        public string PCC_PROJECT_NAME { get; set; }

        /// <summary>
        /// 計畫實際承辦人
        /// </summary>
        public string FACTORY_CONTACT { get; set; }

        /// <summary>
        /// 承辦人電話
        /// </summary>
        public string FACTORY_TEL { get; set; }

        /// <summary>
        /// 決標金額
        /// </summary>
        public decimal TENDER_AWARDING_AMT { get; set; }

        /// <summary>
        /// 發包金額 
        /// </summary>
        public decimal PROCUREMENT_AMT { get; set; }

        /// <summary>
        /// 檢核點資料
        /// </summary>
        public List<ProjectCusCheckpointModel> CustomChkItemModels { get; set; }

        /// <summary>
        /// 工程預定進度表檔案資料
        /// </summary>
        public List<ProjectAttachmentModel> FileModels { get; set; }

        /// <summary>
        /// 是否移除竣工相關資料
        /// </summary>
        public bool DeleteEngData { get; set; }

        /// <summary>
        /// 可否存檔
        /// </summary>
        public bool CanSave { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IS_SEND { get; set; }

        /// <summary>
        /// 取消執行情形送出
        /// </summary>
        public bool CancelSend { get; set; }

        /// <summary>
        /// 移除特定工程進度檔案識別碼
        /// </summary>
        public List<int> RemovedFileIds { get; set; }

        /// <summary>
        /// 總期程/分月期程調整歷程
        /// </summary>
        public List<AdjustScheHistoryModel> AdjustScheHistoryModels { get; set; }

        /// <summary>
        /// 是否可以關聯公會標案
        /// </summary>
        public bool IS_TYCG_PROJECT { get; set; }
        /// <summary>
        /// 計畫調整狀態
        /// </summary>
        public string PROJECT_AW_STATUS { get; set; }
    }
}
