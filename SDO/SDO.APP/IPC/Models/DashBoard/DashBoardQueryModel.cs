using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 儀表板 QueryModel
    /// </summary>
    public class DashBoardQueryModel
    {
        /// <summary>
        /// 民國年
        /// </summary>
        public string DAB_YEAR_YYY { get; set; }
        /// <summary>
        /// 月份(兩碼)
        /// </summary>
        public string DAB_MONTH { get; set; }
        /// <summary>
        /// 執行機關代碼
        /// </summary>
        public string EXEC_ORGAN_C { get; set; }
        /// <summary>
        /// 行政區代碼
        /// </summary>
        public string TOWN_C { get; set; }
        /// <summary>
        /// 是否為執行中列管情形資料
        /// </summary>
        public bool IS_IN_PROGRESS_DATA { get; set; }

        internal void Deconstruct(out string DAB_YEAR_YYY, out string DAB_MONTH, out string EXEC_ORGAN_C, out bool IS_IN_PROGRESS_DATA)
        {
            DAB_YEAR_YYY = this.DAB_YEAR_YYY;
            DAB_MONTH = this.DAB_MONTH;
            EXEC_ORGAN_C = this.EXEC_ORGAN_C;
            IS_IN_PROGRESS_DATA = this.IS_IN_PROGRESS_DATA;
        }
    }
}
