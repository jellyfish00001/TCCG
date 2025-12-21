using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 重大建設分析查詢
    /// </summary>
    public class AnalysisQueryModel
    {
        /// <summary>
        /// 案件類型 0:工程類 1:非工程類
        /// </summary>
        public string CP_KIND { get; set; }

        /// <summary>
        /// 結案狀態 Y:已結案 N:未結案
        /// </summary>
        public string IS_PROJECT_FINISH { get; set; }

        /// <summary>
        /// 計畫經費
        /// </summary>
        public string PROJ_BUDGET { get; set; }

        /// <summary>
        /// 圖表類別 0:建設類別 1:機關: 2:辦理地點
        /// </summary>
        public int CHART_TYPE { get; set; }

        /// <summary>
        /// 主管機關
        /// </summary>
        public string MASTER_DEPT { get; set; }

        /// <summary>
        /// 執行機關
        /// </summary>
        public string EXEC_DEPT { get; set; }

        /// <summary>
        /// 主頁點擊的型態
        /// </summary>
        public string MAIN_TYPE { get; set; }

        /// <summary>
        /// 主頁點擊的值
        /// </summary>
        public string MAIN_VALUE { get; set; }

        /// <summary>
        /// 是否取得符合 Null:無 True:是 False:不
        /// </summary>
        public bool? IS_GET_MATCH { get; set; }

        /// <summary>
        /// 次頁點擊的型態
        /// </summary>
        public string DETAIL_TYPE { get; set; }

        /// <summary>
        /// 次頁點擊的值
        /// </summary>
        public string DETAIL_VALUE { get; set; }

        /// <summary>
		/// 落後類別
		/// </summary>
		public string DELAY_KIND { get; set; }

        /// <summary>
        /// 落後次類別代碼
        /// </summary>
        public string DELAY_SUBCLASS_C { get; set; }

        /// <summary>
        /// 是否只有主辦權限
        /// </summary>
        public bool IS_HAND_ROLE { get; set; }

        /// <summary>
        /// 建立者
        /// </summary>
        public string CRT_USER { get; set; }
        public string OrgId { get; set; }
    }
}
