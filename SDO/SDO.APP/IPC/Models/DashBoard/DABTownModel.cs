using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 歷年列管情形Model
    /// </summary>
    public class DABTownModel
    {
        /// <summary>
        /// 西元年度
        /// </summary>
        public string DAB_YEAR_YYYY {
            get
            {
                int dabYYY = 0;
                if (Int32.TryParse(DAB_YEAR_YYY,out dabYYY))
                    return (dabYYY + 1911).ToString();
                else
                    return string.Empty;
            } }
        /// <summary>
        /// 民國年度
        /// </summary>
        public string DAB_YEAR_YYY { get; set; }
        /// <summary>
        /// 月份
        /// </summary>
        public string DAB_MONTH { get; set; }
        /// <summary>
        /// 執行機關代碼
        /// </summary>
        public string EXEC_ORGAN_C { get; set; }
        /// <summary>
        /// 行政區
        /// </summary>
        public string TOWN { get; set; }
        /// <summary>
        /// 行政區名稱
        /// </summary>
        public string TOWN_NAME { get; set; }
        /// <summary>
        /// 列管總件數
        /// </summary>
        public decimal TOTAL_NUM { get; set; }
        /// <summary>
        /// 施工中案件數
        /// </summary>
        public decimal STAGE_E2_NUM { get; set; }
        /// <summary>
        /// 施工中落後案件數
        /// </summary>
        public decimal STAGE_E2_DELAY_NUM { get; set; }
        /// <summary>
        /// 尚未開工案件數
        /// </summary>
        public decimal STAGE_E1_NUM { get; set; }
        /// <summary>
        /// 尚未開工落後案件數
        /// </summary>
        public decimal STAGE_E1_DELAY_NUM { get; set; }
        /// <summary>
        /// 已竣工但未結案案件數
        /// </summary>
        public decimal STAGE_E3_NUM { get; set; }
        /// <summary>
        /// 已竣工但未結案落後件數
        /// </summary>
        public decimal STAGE_E3_DELAY_NUM { get; set; }
        /// <summary>
        /// 近兩年已完工案件數
        /// </summary>
        public int LastTwoYearsFinishCnt { get; set; }
        /// <summary>
        /// 當年度應完工件數
        /// </summary>
        public int EstimateFinishCnt { get; set; }
        /// <summary>
        /// 當年度已完工件數
        /// </summary>
        public int ActualFinishCnt { get; set; }
    }
}
