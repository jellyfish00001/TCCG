using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 年終考核
    /// </summary>
    public class ProjectFillYearAssModel
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
        /// 計畫狀態
        /// </summary>
        public string PROJECT_STATUS { get; set; }

        /// <summary>
        /// 計畫年度
        /// </summary>
        public int PROJECT_YEAR { get; set; }

        /// <summary>
        /// 立案日期
        /// </summary>
        public DateTime? CREATEDTIME { get; set; }

        /// <summary>
        /// 預定完成期限
        /// </summary>
        public DateTime? PROJECT_LAST_DATE { get; set; }

        /// <summary>
        /// 初審成績(A)
        /// </summary>
        public int SCORE_A { get; set; }

        /// <summary>
        /// 申請分月期程調整次數
        /// </summary>
        public int SCHE_M_CNT { get; set; }

        /// <summary>
        /// 申請總期程調整次數
        /// </summary>
        public int SCHE_Y_CNT { get; set; }

        /// <summary>
        /// 執行方式類別
        /// </summary>
        public string CP_KIND { get; set; }

        /// <summary>
        /// 總期程(CREATEDTIME、PROJECT_LAST_DATE相差月份)
        /// </summary>
        public int ExMonth { get; set; }

        /// <summary>
        /// 執行方式名稱
        /// </summary>
        public string CHECKPOINT_CLASS { get; set; }

        /// <summary>
        /// 累計實際支付數
        /// </summary>
        public decimal ACTUAL_PAY { get; set; }

        /// <summary>
        /// 應付未付數
        /// </summary>
        public decimal UNPAY { get; set; }

        /// <summary>
        /// 結餘數
        /// </summary>
        public decimal BALANCE { get; set; }

        /// <summary>
        /// 計畫開始日期
        /// </summary>
        public DateTime? CONTROL_DATE1 { get; set; }

        /// <summary>
        /// 結案時間
        /// </summary>
        public DateTime? FINISH_DATE { get; set; }
    }
}
