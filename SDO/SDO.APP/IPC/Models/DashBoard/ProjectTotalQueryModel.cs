using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫經費件數統計資料查詢Model
    /// </summary>
    public class ProjectTotalQueryModel
    {
        /// <summary>
        /// 年度
        /// </summary>
        public string DAB_YEAR_YYY { get; set; }
        /// <summary>
        /// 月份
        /// </summary>
        public string DAB_MONTH { get; set; }
        /// <summary>
        /// 統計類別
        /// </summary>
        public string DAB_KIND { get; set; }
        /// <summary>
        /// 計畫狀態種類
        /// 1: 當年度列管 
        /// 2: 執行中
        /// </summary>
        public string ProjectStatusType { get; set; }
    }
}
