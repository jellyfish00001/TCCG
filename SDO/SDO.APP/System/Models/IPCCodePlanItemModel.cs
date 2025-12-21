using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 預算來源
    /// </summary>
    public class IPCCodePlanItemModel : DbEditor
    {
        /// <summary>
        /// 計畫年度
        /// </summary>
        public int PLAN_YEAR { get; set; }

        /// <summary>
        /// 預算編號
        /// </summary>
        public string PLAN_ITEM_ID { get; set; }

        /// <summary>
        /// 舊的預算編號
        /// </summary>
        public string OLD_PLAN_ITEM_ID { get; set; }

        /// <summary>
        /// 預算名稱
        /// </summary>
        public string PLAN_ITEM_NAME { get; set; }

        /// <summary>
        /// 上層計畫項目代碼(桃園不使用)
        /// </summary>
        public string PARENT_ID { get; set; }

        /// <summary>
        /// 經費來源機關(桃園不使用)
        /// </summary>
        public int ORGAN_BUDGET_SEQ { get; set; }

        /// <summary>
        /// 計畫類別經費
        /// </summary>
        public int PLAN_ITEM_BUDGET { get; set; }

        /// <summary>
        /// 預算來源類別
        /// </summary>
        public string LEVEL_MARK { get; set; }

        /// <summary>
        /// 刪除註記
        /// </summary>
        public bool DEL_FLG { get; set; }
    }
}
