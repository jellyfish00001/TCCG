using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class ProjectBasicFillModel 
    {
        /// <summary>
        /// 計畫基本資料
        /// </summary>
        public ProjectBasicModel ProjectBasic { get; set; }

        /// <summary>
        /// 計畫經費來源
        /// </summary>
        public List<ProjectBudgetSourceGModel> ProjectBudgetSourceG { get; set; }

        /// <summary>
        /// 計畫建設類別
        /// </summary>
        public List<ProjectBuildKindModel> ProjectBuildKind { get; set; }

        /// <summary>
        /// 計畫協辦機關
        /// </summary>
        public List<ProjectAsstOrgModel> ProjectAsstOrg { get; set; }
    }
}
