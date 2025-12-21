using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 表4 每月案件落後挑案列表
    /// </summary>
    public class ProjectDelayListModel
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 落後資料年分
        /// </summary>
        public string DATA_YEAR { get; set; }

        /// <summary>
        /// 落後資料月份
        /// </summary>
        public int DATA_MONTH { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
	    public string PROJECT_NAME { get; set; }

        /// <summary>
        /// 執行機關名稱
        /// </summary>
        public string EXEC_ORGAN_NAME { get; set; }

        /// <summary>
        /// 計畫總經費
        /// </summary>
        public long BUDGET { get; set; }

        /// <summary>
        /// 預定施工進度
        /// </summary>
        public decimal? IPC_RES_PRG { get; set; }

        /// <summary>
        /// 實際施工進度
        /// </summary>
        public decimal? IPC_ACT_PRG { get; set; }

        /// <summary>
        /// 執行情形說明
        /// </summary>
        public string EXECUTE_CONDITION { get; set; }

        /// <summary>
        /// 落後類型
        /// </summary>
        public string DELAY_KIND { get; set; }

        /// <summary>
        /// 落後原因
        /// </summary>
        public string DELAY_CAUSAL { get; set; }

        /// <summary>
        /// 機關排序
        /// </summary>
        public string OU_SORT_ORDER { get; set; }

        /// <summary>
		/// 執行方式類別
		/// </summary>
		public string CP_KIND { get; set; }

        /// <summary>
        /// 實際完成日期
        /// </summary>
        public DateTime? ACTUAL_ENDDATE { get; set; }
    }
}
