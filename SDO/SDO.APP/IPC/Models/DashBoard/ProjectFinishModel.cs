using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫完工資料
    /// </summary>
    public class ProjectFinishModel
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 預計完成日期
        /// </summary>
        public DateTime? ESTIMATED_ENDDATE { get; set; }
        /// <summary>
        /// 實際完成日期
        /// </summary>
        public DateTime? FINISH_DATE { get; set; }
    }
}
