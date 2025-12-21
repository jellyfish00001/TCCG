using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 委託研究的基本資料 Model
    /// </summary>
    public class ResearchBasicModel : RDProjectManageModel
    {
        /// <summary>
        /// 送審資料
        /// </summary>
        public RDAuditStatusModel AUDIT { get; set; }

        /// <summary>
        /// 每季評核指標
        /// </summary>
        public List<PolicyIndexModel> policyIndex{ get; set; }

        /// <summary>
        /// 基本資料審查狀態
        /// </summary>
        public string RESEARCH_STATUS { get; set; }

        /// <summary>
        /// 公務預算千元
        /// </summary>
        public decimal PUBLIC_MONEY { get; set; }

        /// <summary>
        /// 基金預算千元
        /// </summary>
        public decimal FUND_MONEY { get; set; }

        /// <summary>
        /// 中央預算千元
        /// </summary>
        public decimal CENTER_MONEY { get; set; }

        /// <summary>
        /// 其他預算千元
        /// </summary>
        public decimal OTHER_MONEY { get; set; }

        /// <summary>
        /// 其他預算說明
        /// </summary>
        public string OTHER_DESC { get; set; }

        /// <summary>
        /// 研究原因及目的
        /// </summary>
        public string PLAN_CAUSE { get; set; }

        /// <summary>
        /// 計畫項目內容
        /// </summary>
        public string PLAN_CONTENT { get; set; }

        /// <summary>
        /// 預期研究成果
        /// </summary>
        public string PLAN_EXPECTED { get; set; }

        /// <summary>
        /// 電話
        /// </summary>
        public string CONTACT_TEL { get; set; }

        /// <summary>
        /// 承辦人EMAIL
        /// </summary>
        public string CONTACT_EMAIL { get; set; }

        // <summary>
        /// 代理人EMAIL
        /// </summary>
        public string ASSIGNE_EMAIL { get; set; }

        /// <summary>
        /// 檔案上傳
        /// </summary>
        public ProjectAttachmentModel FILE { set; get; }
    }
}
