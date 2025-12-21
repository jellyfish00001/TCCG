using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 機關連續落後比率
    /// </summary>
    public class DABDelayMonthModel
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
        /// 月份
        /// </summary>
        public string DAB_MONTH { get; set; }
        /// <summary>
        /// 資料日
        /// </summary>
        public DateTime DabDate
        {
            get
            {
                if (Int32.TryParse(DAB_YEAR_YYYY, out int dabYear)
                    && Int32.TryParse(DAB_MONTH, out int dabMonth))
                {
                    return new DateTime(dabYear, dabMonth, 1);
                }
                return DateTime.Now;
            }
        }
        /// <summary>
        /// 執行機關代碼
        /// </summary>
        public string EXEC_ORGAN_C { get; set; }
        /// <summary>
        /// 執行機關名稱
        /// </summary>
        public string EXEC_DEPT { get; set; }
        /// <summary>
        /// 列管總件數
        /// </summary>
        public decimal TOTAL_NUM { get; set; }
        /// <summary>
        /// 落後件數 
        /// </summary>
        public decimal DELAY_NUM { get { return DELAY_NUM_1 + DELAY_NUM_2 + DELAY_NUM_3; } }
        /// <summary>
        /// 落後比率 (落後件數 /總列管件數)
        /// </summary>
        public decimal DELAY_RATE { 
            get
            {
                return TOTAL_NUM == 0 ? 0 : Math.Round(DELAY_NUM / TOTAL_NUM, 2);
            }
        }
        /// <summary>
        /// 連續落後3個月件數
        /// </summary>
        public decimal DELAY_NUM_3 { get; set; }
        /// <summary>
        /// 連續落後3個月比率
        /// </summary>
        public decimal DELAY_NUM_3_RATE { get; set; }
        /// <summary>
        /// 連續落後2個月件數
        /// </summary>
        public decimal DELAY_NUM_2 { get; set; }
        /// <summary>
        /// 連續落後2個月比率
        /// </summary>
        public decimal DELAY_NUM_2_RATE { get; set; }
        /// <summary>
        /// 連續落後1個月件數
        /// </summary>
        public decimal DELAY_NUM_1 { get; set; }
        /// <summary>
        /// 連續落後1個月比率
        /// </summary>
        public decimal DELAY_NUM_1_RATE { get; set; }
        /// <summary>
        /// 是否為統計執行中計畫
        /// </summary>
        public bool IS_IN_PROGRESS_DATA { get; set; }
    }
}
