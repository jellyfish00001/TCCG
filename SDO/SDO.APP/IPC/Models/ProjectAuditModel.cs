using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫審查資料檔Model
    /// </summary>
    public class ProjectAuditModel : DbEditor
    {
        /// <summary>
        /// 流水編號
        /// </summary>
        public int LOG_ID { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 審查類別
        /// </summary>
        public string PLAN_REVIEW_TYPE { get; set; }

        /// <summary>
        /// 審查結果
        /// </summary>
        public string REVIEW_RESULT { get; set; }

        /// <summary>
        /// 審查意見
        /// </summary>
        public string REVIEW_COMMENTS { get; set; }
        /// <summary>
        /// 是否確認送出
        /// </summary>
        public bool IS_SEND { get; set; }
    }
}
