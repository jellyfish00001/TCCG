using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫經費件數統計資料
    /// </summary>
    public class ProjectTotalDataModel : DbEditor
    {
        /// <summary>
        /// 統計類別 (建設類別、行政區)
        /// </summary>
        public string DAB_KIND { get; set; }
        /// <summary>
        /// 執行機關/建設類別/行政區 代碼
        /// </summary>
        public string SET_TYPE { get; set; }
        /// <summary>
        /// 執行機關/建設類別/行政區 名稱
        /// </summary>
        public string SET_VALUE { get; set; }

        /// <summary>
        /// 件數
        /// </summary>
        public double TOTAL_NUM { get; set; }
        /// <summary>
        /// 經費資料
        /// </summary>
        public double BUDGET_TOTAL { get; set; }
    }
}
