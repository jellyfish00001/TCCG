using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 表8 落後案件特定檢核點逾期情形
    /// </summary>
    public class SpecChkPointOverdueSituationModel
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

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
        /// 預計完成日期
        /// </summary>
        public DateTime? ESTIMATED_ENDDATE { get; set; }

        /// <summary>
        /// 實際完成日期
        /// </summary>
        public DateTime? ACTUAL_ENDDATE { get; set; }

        /// <summary>
        /// 檢核點名稱
        /// </summary>
        public string CHECKITEM_NAME { get; set; }

        /// <summary>
        /// 控制點
        /// </summary>
        public string CTRL_POINT { get; set; }

        /// <summary>
        /// 機關排序
        /// </summary>
        public string OU_SORT_ORDER { get; set; }

        /// <summary>
        /// 計劃狀態
        /// </summary>
        public string PROJECT_STATUS { get; set; }
    }
}
