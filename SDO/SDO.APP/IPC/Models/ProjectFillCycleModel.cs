using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫填報周期Model
    /// </summary>
    public class ProjectFillCycleModel
    {
        /// <summary>
        /// 流水號 Identity
        /// </summary>
        public int SEQ { get; set; }

        /// <summary>
        /// 計畫填報年度
        /// </summary>
        public string PROJECT_YEAR { get; set; }

        /// <summary>
        /// 計畫填報月份
        /// </summary>
        public string PROJECT_MONTH{ get; set; }

        /// <summary>
        /// 填報開始日期
        /// </summary>
        public DateTime FILL_START_DATE { get; set; }

        /// <summary>
        /// 填報結束日期
        /// </summary>
        public DateTime FILL_END_DATE { get; set; }

        /// <summary>
        /// 填報週期年度
        /// </summary>
        public int PROJECT_YEAR_INT
        {
            get { return int.TryParse(PROJECT_YEAR, out int year) ? year : 0; }
        }

        /// <summary>
        /// 填報週期月份
        /// </summary>
        public int PROJECT_MONTH_INT
        {
            get { return int.TryParse(PROJECT_MONTH, out int month) ? month : 0; }
        }
    }
}
