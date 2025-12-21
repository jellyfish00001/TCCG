using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 逾期繳交填報紀錄
    /// </summary>
    public class ProjectDelayfillModel : DbEditor
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public int SEQ { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 發生時間
        /// </summary>
        public DateTime? FILL_TIME { get; set; }

        /// <summary>
        /// 逾期說明
        /// </summary>
        public string FILL_REASON { get; set; }
    }
}
