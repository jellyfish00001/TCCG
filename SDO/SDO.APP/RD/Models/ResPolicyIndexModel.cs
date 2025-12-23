using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 執行情形明細 Model
    /// </summary>
    public class ResPolicyIndexModel : ResPolicyListQueryModel
    {
        /// <summary>
        /// 送審資料
        /// </summary>
        public RDAuditStatusModel AUDIT { get; set; }
        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PLAN_NAME { get; set; }

        /// <summary>
        /// 委託單位
        /// </summary>
        public string ENTRUST_UNIT_NAME { get; set; }

        /// <summary>
        /// 研究計化主持人
        /// </summary>
        public string RESEARCH_NAME { get; set; }

        /// <summary>
        /// 執行情形簡述
        /// </summary>
        public string EXECUTION_DESC { get; set; }

        /// <summary>
        /// 執行進度
        /// </summary>
        public string PROGRESS_TYPE { get; set; }

        /// <summary>
        /// 落後原因
        /// </summary>
        public string BEHIND_REASON { get; set; }

        /// <summary>
        /// 解決對策
        /// </summary>
        public string SOLUTIONS { get; set; }

        /// <summary>
        /// 期中報告提出日
        /// </summary>
        public string MID_ATTACHMENT_NAME { get; set; }

        /// <summary>
        /// 期末報告提出日
        /// </summary>
        public string FINAL_ATTACHMENT_ID { get; set; }

        /// <summary>
        /// 決標日
        /// </summary>
        public string AWARD_DATE { get; set; }

        /// <summary>
        /// 審查意見
        /// </summary>
        public string REVIEW_DESC { get; set; }

        /// <summary>
        /// 審查結果
        /// </summary>
        public string RESULT_TYPE { get; set; }

    }
}
