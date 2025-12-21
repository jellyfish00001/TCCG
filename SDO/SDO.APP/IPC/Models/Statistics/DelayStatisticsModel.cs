using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 表4 連續落後統計表
    /// </summary>
    public class DelayStatisticsModel
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 落後年度
        /// </summary>
        public int DATA_YEAR { get; set; }
        /// <summary>
        /// 落後月份
        /// </summary>
        public string DATA_MONTH { get; set; }
        /// <summary>
        /// 計畫名稱
        /// </summary>
	    public string PROJECT_NAME { get; set; }
        /// <summary>
        /// 執行機關代碼
        /// </summary>
        public string EXEC_ORGAN_C { get; set; }
        /// <summary>
        /// 執行機關名稱
        /// </summary>
        public string EXEC_ORGAN_NAME { get; set; }
        /// <summary>
        /// 落後類型
        /// </summary>
        public string DELAY_KIND { get; set; }
        /// <summary>
        /// 計劃狀態
        /// </summary>
        public string PROJECT_STATUS { get; set; }
    }
}
