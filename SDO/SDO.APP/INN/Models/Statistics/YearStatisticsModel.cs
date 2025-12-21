using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 表3 各年度提案數統計表
    /// </summary>
    public class YearStatisticsModel
    {
        /// <summary>
        /// 機關別
        /// </summary>
        public string INN_YEAR { get; set; }

        /// <summary>
        /// 組織
        /// </summary>
        public string COUNT_SPONSOR_ORG { get; set; }

        /// <summary>
        /// 區公所
        /// </summary>
        public string COUNT_SPONSOR_DISTRICT_OFFICE { get; set; }

        /// <summary>
        /// 提案人數-個人
        /// </summary>
        public int COUNT_SPONSOR_TYPE_1 { get; set; }

        /// <summary>
        /// 提案人數-團體
        /// </summary>
        public int COUNT_SPONSOR_TYPE_2 { get; set; }

        /// <summary>
        /// 主要提案人性別-男
        /// </summary>
        public int COUNT_SPONSOR_SEX_1 { get; set; }

        /// <summary>
        /// 主要提案人性別-女
        /// </summary>
        public int COUNT_SPONSOR_SEX_2 { get; set; }

        /// <summary>
        /// 提案數
        /// </summary>
        public int PLAN_SUM { get; set; }



    }
}
