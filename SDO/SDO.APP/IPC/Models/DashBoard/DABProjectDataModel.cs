using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 基本資料明細檔
    /// </summary>
    public class DABProjectDataModel:DbEditor
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public int SEQ { get; set; }
        /// <summary>
        /// 西元年
        /// </summary>
        public string DAB_YEAR_YYYY { get; set; }
        /// <summary>
        /// 民國年
        /// </summary>
        public string DAB_YEAR_YYY { get; set; }
        /// <summary>
        /// 月份
        /// </summary>
        public string DAB_MONTH { get; set; }
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
        /// 計畫狀態
        /// </summary>
        public string PROJECT_STATUS { get; set; }
        public string STATUS
        {
            get
            {
                string result = "";
                switch (PROJECT_STATUS)
                {
                    case "7": result = "B"; break;
                    case "8": result = "E"; break;
                    default:
                        switch (DELAY_TYPE)
                        {
                            case "": result = "C"; break;
                            case "D1": result = "D1"; break;
                            case "D2": result = "D2"; break;
                            case "D3": result = "D3"; break;
                        }
                        break;
                }
                return result;
            }
        }
        /// <summary>
        /// 計畫類型
        /// </summary>
        public string PROJECT_CATEGORY { get; set; }
        /// <summary>
        /// 執行機關代碼
        /// </summary>
        public string EXEC_ORGAN_C { get; set; }
        /// <summary>
        /// 執行機關名稱
        /// </summary>
        public string EXEC_DEPT { get; set; }
        /// <summary>
        /// 落後類型
        /// </summary>
        public string DELAY_TYPE { get; set; }
        /// <summary>
        /// 計畫總經費
        /// </summary>
        public double BUDGET_TOTAL { get; set; }
        /// <summary>
        /// 執行機關排序
        /// </summary>
        public string EXEC_SORT_ORDER { get; set; }
        /// <summary>
        /// 主管機關代碼
        /// </summary>
        public string MASTER_ORGAN_C { get; set; }
        /// <summary>
        /// 主管機關
        /// </summary>
        public string MASTER_DEPT { get; set; }
        /// <summary>
        /// 行政區代碼
        /// </summary>
        public string TOWN_C { get; set; }
        /// <summary>
        /// 行政區代碼(跨區:多筆)
        /// </summary>
        public string TOWN_M { get; set; }
        /// <summary>
        /// 行政區名稱
        /// </summary>
        public string TOWNNAME { get; set; }
        /// <summary>
        /// 執行方式
        /// </summary>
        public string CP_KIND { get; set; }
        /// <summary>
        /// 建設類別
        /// </summary>
        public string BUILD_KIND { get; set; }
        /// <summary>
        /// 建設類別名稱
        /// </summary>
        public string BUILD_KIND_DESC { get; set; }
        /// <summary>
        /// 中央補助款
        /// </summary>
        public decimal BUDGET_CENTRAL { get; set; }
        /// <summary>
        /// 計畫檢核點落後天數
        /// </summary>
        public int CHKPT_DELAY_DAYS { get; set; }
        /// <summary>
        /// 進度落後百分比
        /// </summary>
        public decimal DELAY_PRG { get; set; }
    }
}
