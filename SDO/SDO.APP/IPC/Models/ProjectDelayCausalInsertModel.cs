using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class ProjectDelayCausalInsertModel : DbEditor
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 年份
        /// </summary>
        public string DATA_YEAR { get; set; }

        /// <summary>
        /// 月份
        /// </summary>
        public string DATA_MONTH { get; set; }

        /// <summary>
        /// 落後原因類型
        /// </summary>
        public string DELAY_KIND { get; set; }
    }
}
