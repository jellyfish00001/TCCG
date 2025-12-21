using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 每月綜合排序
    /// </summary>
    public class DABCompositeModel:DbEditor
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public int SEQ { get; set; }
        /// <summary>
        /// 西元年度
        /// </summary>
        public string DAB_YEAR_YYYY { get; set; }
        /// <summary>
        /// 民國年度
        /// </summary>
        public string DAB_YEAR_YYY { get; set; }
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
        /// 月份
        /// </summary>
        public string DAB_MONTH { get; set; }
        /// <summary>
        /// 報表類別
        /// </summary>
        public string DAB_KIND { get; set; }
        /// <summary>
        /// 代碼
        /// 執行機關、行政區、建設類別
        /// </summary>
        public string SET_TYPE { get; set; }
        /// <summary>
        /// 名稱
        /// 執行機關、行政區、建設類別
        /// </summary>
        public string SET_VALUE { get; set; }
        /// <summary>
        /// 總列管件數(A)
        /// </summary>
        public double TOTAL_NUM { get; set; }

        /// <summary>
        /// 各類別項目總經費
        /// </summary>
        public double BUDGET_TOTAL { get; set; }
        /// <summary>
        /// 已結案件數(B)
        /// </summary>
        public int CLOSE_NUM { get; set; }
        /// <summary>
        /// 進度符合或超前(C)
        /// </summary>
        public int CONFORM_NUM { get; set; }
        /// <summary>
        /// 執行進度落後案件數(D)
        /// </summary>
        public double DELAY_NUM { get; set; }
        /// <summary>
        /// 執行進度落後件數排序(F1)
        /// </summary>
        public int F1 { get; set; }
        /// <summary>
        /// 執行進度落後比率(D/A)
        /// </summary>
        public decimal DELAY_RATE { get; set; }
        /// <summary>
        /// 執行進度落後比率排序(F2)
        /// </summary>
        public int F2 { get; set; }
        /// <summary>
        /// 執行進度落後_D1(D1)
        /// </summary>
        public int DELAY_NUM_D1 { get; set; }
        /// <summary>
        /// 執行進度落後_D2(D2)
        /// </summary>
        public int DELAY_NUM_D2 { get; set; }
        /// <summary>
        /// 執行進度落後_D3(D3)
        /// </summary>
        public int DELAY_NUM_D3 { get; set; }
        /// <summary>
        /// 撤銷列管件數(E)
        /// </summary>
        public int CANCEL_NUM { get; set; }
        /// <summary>
        /// 完成率(B/A)
        /// </summary>
        public decimal COMPLETE_RATE { get; set; }
        /// <summary>
        /// 落後排序合計(F1+F2)
        /// </summary>
        public int DELAY_F1F2
        {
            get
            {
                return F1 + F2;
            }
        }
        /// <summary>
        /// 落後綜合排序
        /// </summary>
        public decimal DELAY_RANKING { get; set; }
        /// <summary>
        /// 可能影響補助款計畫數
        /// </summary>
        public int AFFECTED_SUBSIDY_NUM { get; set; }
        /// <summary>
        /// 是否為統計執行中計畫
        /// </summary>
        public bool IS_IN_PROGRESS_DATA { get; set; }
    }
}
