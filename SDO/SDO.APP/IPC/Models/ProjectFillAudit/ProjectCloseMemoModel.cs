using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 結案意見
    /// </summary>
    public class ProjectCloseMemoModel : DbEditor
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 合計扣減
        /// </summary>
        public int TOTAL_SCORE { get; set; }
    }
}
