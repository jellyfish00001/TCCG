using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 展延計畫紀錄 Model
    /// </summary>
    public class ProjectExtensionModel : DbEditor
    {
        /// <summary>
        /// 送審資料
        /// </summary>
        public RDAuditStatusModel AUDIT { get; set; }
        /// <summary>
        /// 展延序號
        /// </summary>
        public int EXTENSION_ID { get; set; }

        /// <summary>
        /// 展延編號
        /// </summary>
        public string EXTENSION_NO { get; set; }

        /// <summary>
        /// 展延原因
        /// </summary>
        public string EXT_REASON { get; set; }

        /// <summary>
        /// 原計畫期程 - 起
        /// </summary>
        public DateTime? PLAN_START_DATE { get; set; }

        /// <summary>
        /// 原計畫期程 - 迄
        /// </summary>
        public DateTime? PLAN_END_DATE { get; set; }

        /// <summary>
        /// 調整計畫期程 - 迄
        /// </summary>
        public DateTime? EXTP_LANEND_DATE { get; set; }

        /// <summary>
        /// 申請時間
        /// </summary>
        public DateTime? APPLY_DATE { get; set; }

        /// <summary>
        /// 申請人
        /// </summary>
        public string APPLICANT { get; set; }

        /// <summary>
        /// 展延申請狀態 - 轉參數名稱
        /// </summary>
        public string EXTENSION_STATUS { get; set; }

        /// <summary>
        /// 展延申請狀態 - 代碼
        /// </summary>
        public string EXTENSION_STATUS_CODE { get; set; }

        /// <summary>
        /// 檔案上傳
        /// </summary>
        public ProjectAttachmentModel FILE { set; get; }

        /// <summary>
        /// 展延紀錄評核指標
        /// </summary>
        public List<ExtensionPolicyIndexModel> policyIndex { get; set; }

        /// <summary>
        /// 展延申請年度
        /// </summary>
        /// <remarks>產生展延編號時需要使用到當下民國年度</remarks>
        public string EXTENSION_YEAR { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        /// <remarks>撈取評核指標時需要用到計畫編號</remarks>
        public string PLAN_NO{ get; set; }

        /// <summary>
        /// 計畫序號
        /// </summary>
        /// <remarks>評核指標調整表回存評核指標需用到</remarks>
        public int PLAN_ID { get; set; }

        /// <summary>
        /// 結案成果填報是否有資料，1:有資料 0:無資料
        /// </summary>
        /// <remarks>>透過此值判斷是否可申請展延</remarks>
        //public int IS_RES_EXTENSION_EXISTS { get; set; }

        /// <summary>
        /// 評核指標數量
        /// </summary>
        /// <remarks>>透過此值判斷是否可申請展延</remarks>
        public int RD_RES_POLICY_INDEX_COUNT { get; set; }

        /// <summary>
        /// 評核指標審核通過數量
        /// </summary>
        /// <remarks>>透過此值判斷是否可申請展延</remarks>
        public int STATUS_3_COUNT { get; set; }
    }
}
