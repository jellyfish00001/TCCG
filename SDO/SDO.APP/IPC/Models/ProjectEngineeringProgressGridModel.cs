using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class ProjectEngineeringProgressGridModel : ProjectEngineeringProgressModel
    {
        /// <summary>
        /// 標案預定施工進度
        /// </summary>
        public decimal? TEN_RES_PRG { get; set; }

        /// <summary>
        /// 標案實際施工進度
        /// </summary>
        public decimal? TEN_ACT_PRG { get; set; }

        /// <summary>
        /// 填報日期
        /// </summary>
        public DateTime? SEND_DATE { get; set; }

        /// <summary>
        /// 逾期天數
        /// </summary>
        public int OVERDUE_DAY { get; set; }

        /// <summary>
        /// 不計算逾期天數	0否(預設)/1是
        /// </summary>
        public bool DISREGARD { get; set; }
    }
}
