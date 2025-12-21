using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 基本資料 - 評核指標
    /// </summary>
    public class PolicyIndexModel : DbEditor
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public int SEQ { get; set; }

        /// <summary>
        /// 評核指標
        /// </summary>
        public string POLICY_INDEX_DESC { get; set; }

        /// <summary>
        /// 計畫序號
        /// </summary>
        public int PLAN_ID { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PLAN_NO { get; set; }

        /// <summary>
        /// 評核類別
        /// </summary>
        public string POLICY_KIND { get; set; }

        /// <summary>
        /// 預定完成期程
        /// </summary>
        public string RES_FINISH_DATE { get; set; }

        /// <summary>
        /// 預定完成期程
        /// </summary>
        public string RES_EXTP_LANEND_DATE { get; set; }
    }
}
