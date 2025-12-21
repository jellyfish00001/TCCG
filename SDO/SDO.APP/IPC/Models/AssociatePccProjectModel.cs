using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 有設定關聯工程會標案系統的計畫資料Model
    /// </summary>
    public class AssociatePccProjectModel
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
        /// 計畫年度
        /// </summary>
        public string PROJECT_YEAR { get; set; }

        /// <summary>
        /// 是否使用標案系統資料
        /// </summary>
        public bool IS_USER_FTY_DATA { get; set; }

        /// <summary>
        /// 標案UID
        /// </summary>
        public string PCC_PROJECT_UID { get; set; }

        /// <summary>
        /// 填報年度(民國年)
        /// </summary>
        public string YEAR { get; set; }

        /// <summary>
        /// 填報月份
        /// </summary>
        public string MONTH { get; set; }

        /// <summary>
        /// 是否送出
        /// </summary>
        public bool IS_SEND { get; set; }

        /// <summary>
        /// 標案預定施工進度
        /// </summary>
        public decimal? TEN_RES_PRG { get; set; }

        /// <summary>
        /// 標案實際施工進度
        /// </summary>
        public decimal? TEN_ACT_PRG { get; set; }

    }
}
