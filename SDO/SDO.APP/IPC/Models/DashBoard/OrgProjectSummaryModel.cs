
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 機關統計資料
    /// </summary>
    public class OrgProjectSummaryModel
    {
        /// <summary>
        /// 列管件數
        /// </summary>
        public double ProjectCount { get; set; }
        /// <summary>
        /// 較上個月列管件數差異數
        /// </summary>
        public double ProjectCountDiff { get; set; }
        /// <summary>
        /// 計畫總經費
        /// </summary>
        public decimal BudgetTotal { get; set; }
        /// <summary>
        /// 較上個月計畫總經費差異數
        /// </summary>
        public decimal BudgetTotalDiff { get; set; }
        /// <summary>
        /// 落後件數
        /// </summary>
        public double DelayCount { get; set; }
        /// <summary>
        /// 較上個月落後件數差異數
        /// </summary>
        public double DelayCountDiff { get; set; }
        /// <summary>
        /// 落後比率
        /// </summary>
        public decimal DelayRate { get; set; }
        /// <summary>
        /// 較上個月落後比率差異數
        /// </summary>
        public decimal DelayRateDiff { get; set; }
        /// <summary>
        /// 連續落後三個月以上案件比率
        /// </summary>
        public decimal DelayOver3MonthsRate { get; set; }
        /// <summary>
        /// 較上個月連續落後三個月以上案件比率差異數
        /// </summary>
        public decimal DelayOver3MonthsRateDiff { get; set; }
        /// <summary>
        /// 近一年落後比率資料(機關)
        /// </summary>
        public List<DABCompositeModel> AnnualOrgDelayRateData { get; set; }
        /// <summary>
        /// 計畫各階段落後資料
        /// </summary>
        public List<DABProjectDataModel> ProjectCategoryDelayData { get; set; }
        /// <summary>
        /// 行政區落後情形資料
        /// </summary>
        public List<TownDelayModel> TownDelayData { get; set;}
        /// <summary>
        /// 可能影響補助款件數
        /// </summary>
        public int AffectedSubsidyNum { get; set; }
        /// <summary>
        /// 工程進度落後比率低於10%件數
        /// </summary>
        public int DelayPrgBelow10Cnt { get; set; }
        /// <summary>
        /// 工程進度落後比率高於10%且低於20%件數
        /// </summary>
        public int DelayPrgBelow20Cnt { get; set; }
        /// <summary>
        /// 工程進度落後比率高於20%件數
        /// </summary>
        public int DelayPrgOver20Cnt { get; set; }
        /// <summary>
        /// 檢核點落後未達3個月件數
        /// </summary>
        public int ChkPtDelayBelow3MonthsCnt { get; set; }
        /// <summary>
        /// 檢核點落後3個月以上且未達6個月件數
        /// </summary>
        public int ChkPtDelayBelow6MonthsCnt { get; set; }
        /// <summary>
        /// 檢核點落後超過6個月件數
        /// </summary>
        public int ChkPtDelayOver6MonthsCnt { get; set; }
    }
}
