using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 工程標案各月執行進度 (工程會API欄位)
    /// </summary>
    public class PccmDs04Model
    {
        /// <summary>
        /// 標案識別碼
        /// </summary>
        public string plnprj_uid { get; set; }
        /// <summary>
        /// 標案代碼
        /// </summary>
        public string plnprj_id { get; set; }
        /// <summary>
        /// 標案名稱
        /// </summary>
        public string plnprj_name { get; set; }
        /// <summary>
        /// 執行單位代碼
        /// </summary>
        public string execorg_code { get; set; }
        /// <summary>
        /// 執行單位名稱
        /// </summary>
        public string execorg_name { get; set; }
        /// <summary>
        /// 年
        /// </summary>
        public int plnprj_year { get; set; }
        /// <summary>
        /// 月份
        /// </summary>
        public string plnprj_month { get; set; }
        /// <summary>
        /// 各月累計預定進度(%)
        /// </summary>
        public decimal monthscheduledprogress { get; set; }
        /// <summary>
        /// 各月累計實際進度(%)
        /// </summary>
        public decimal monthactualprogress { get; set; }
        /// <summary>
        /// 年累計預定進度(%)
        /// </summary>
        public decimal yearscheduledprogress { get; set; }
        /// <summary>
        /// 年累計實際進度(%)
        /// </summary>
        public decimal yearactualprogress { get; set; }
        /// <summary>
        /// 實際工作摘要
        /// </summary>
        public string remark { get; set; }
        /// <summary>
        /// 最後更新時間
        /// </summary>
        public string updatetime { get; set; }
    }
}
