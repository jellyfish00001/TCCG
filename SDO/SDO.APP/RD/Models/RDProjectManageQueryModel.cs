using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 委託研究管理清單 查詢條件
    /// </summary>
    public class RDProjectManageQueryModel : DbEditor
    {
        /// <summary>
        /// 計畫年度
        /// </summary>
        public string PLAN_YEAR { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PLAN_NO { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PLAN_NAME { get; set; }

        /// <summary>
        /// 受託單位
        /// </summary>
        public string ENTRUST_UNIT_NAME { get; set; }

        /// <summary>
        /// 執行機關代號
        /// </summary>
        public string OU_ID { get; set; }
    }
}
