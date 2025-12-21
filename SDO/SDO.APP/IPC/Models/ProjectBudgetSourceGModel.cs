using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫經費來源
    /// </summary>
    public class ProjectBudgetSourceGModel : DbEditor
    {
        /// <summary>
        /// IDENTITY_FIELD
        /// </summary>
        public int IDENTITY_FIELD { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 年度
        /// </summary>
        public string PLAN_YEAR { get; set; }

        /// <summary>
        /// 計畫類型(預算類型) 0一般、1前瞻
        /// </summary>
        public string BUDGET_CLASS { get; set; }

        /// <summary>
        /// 中央預算來源
        /// </summary>
        public string PLAN_ITEM_C { get; set; }

        /// <summary>
        /// 中央補助款(元)
        /// </summary>
        public long BUDGET_CENTRAL { get; set; }

        /// <summary>
        /// 本府預算來源
        /// </summary>
        public string PLAN_ITEM_L { get; set; }

        /// <summary>
        /// 地方自籌款(元)
        /// </summary>
        public long BUDGET_LOCAL { get; set; }

        /// <summary>
        /// 核定函檔案
        /// </summary>
        public List<ProjectAttachmentModel> FILE { get; set; }
    }
}
