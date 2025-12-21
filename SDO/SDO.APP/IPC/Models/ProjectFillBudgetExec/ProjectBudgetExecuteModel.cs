using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 預算執行情形累計執行情形
    /// </summary>
    public class ProjectBudgetExecuteModel : DbEditor
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public int SEQ { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 年
        /// </summary>
        public string EXEC_YEAR { get; set; }

        /// <summary>
        /// 月
        /// </summary>
        public string EXEC_MONTH { get; set; }

        /// <summary>
        /// 累計預定支用(a)
        /// </summary>
        public decimal GT_EXPANDED_BUDGET { get; set; }

        /// <summary>
        /// 累計實際支用(b)
        /// </summary>
        public decimal GT_ACT_BUDGET { get; set; }

        /// <summary>
        /// 應付未付數(c)
        /// </summary>
        public decimal GT_AP { get; set; }

        /// <summary>
        /// 結餘數(d)
        /// </summary>
        public decimal GT_BALANCE { get; set; }

        /// <summary>
        /// 累計支用總數
        /// </summary>
        public decimal GT_TOTAL { get; set; }

        /// <summary>
        /// 累計預算執行率(b+c+d/a)
        /// </summary>
        public decimal GT_EXEC_RATE { get; set; }

        /// <summary>
        /// 本年度執行情形可支用預算數(j)
        /// </summary>
        public decimal YEAR_BUDGET_EXPANDED { get; set; }

        /// <summary>
        /// 本年度執行情形預算分配數(k)
        /// </summary>
        public decimal YEAR_BUDGET_ALLOCATED { get; set; }

        /// <summary>
        /// 本年度執行情形預算執行數(l)
        /// </summary>
        public decimal YEAR_EXEC_BUDGET { get; set; }

        /// <summary>
        /// 本年度執行情形執行率(m=l/k)
        /// </summary>
        public decimal YEAR_EXEC_RATE { get; set; }

        /// <summary>
        /// 預算執行率未達80%說明
        /// </summary>
        public string EXEC_RATE_FAILED_NOTE { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        public string EXEC_NOTE { get; set; }

        /// <summary>
        /// 填報日期
        /// </summary>
        public DateTime CRT_DATE { get; set; }

        /// <summary>
        /// 原因 計畫參數值對應資料
        /// </summary>
        public List<ProjectMappingDataModel> FailedMappingData { get; set; }

        /// <summary>
        /// 責任歸屬 計畫參數值對應資料
        /// </summary>
        public List<ProjectMappingDataModel> FailedDutyMappingData { get; set; }
    }
}
