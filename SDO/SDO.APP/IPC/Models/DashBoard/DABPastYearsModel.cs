using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 歷年列管情形Model
    /// </summary>
    public class DABPastYearsModel
    {
        /// <summary>
        /// 西元年度
        /// </summary>
        public string DAB_YEAR_YYYY { get; set; }
        /// <summary>
        /// 民國年度
        /// </summary>
        public string DAB_YEAR_YYY { get; set; }
        /// <summary>
        /// 執行機關代碼
        /// </summary>
        public string EXEC_ORGAN_C { get; set; }
        /// <summary>
        /// 執行機關名稱
        /// </summary>
        public string EXEC_DEPT { get; set; }
        /// <summary>
        /// 立案件數
        /// </summary>
        public decimal TOTAL_NUM { get; set; }
        /// <summary>
        /// 立案總金額
        /// </summary>
        public decimal TOTAL_EXS { get; set; }
        /// <summary>
        /// 落後比率
        /// </summary>
        public decimal DELAY_RATE { get; set; }
    }
}
