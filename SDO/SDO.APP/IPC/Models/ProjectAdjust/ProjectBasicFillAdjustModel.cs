using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.IPC.Models.ProjectAdjust
{
    public class ProjectBasicFillAdjustModel : ProjectBasicFillModel
    {
        /// <summary>
        /// 計畫基本資料
        /// </summary>
        public new ProjectBasicAdjustModel ProjectBasic { get; set; }
        /// <summary>
        /// 計畫建設類別
        /// </summary>
        public new List<ProjectBuildKindAdjustModel> ProjectBuildKind { get; set; }
        /// <summary>
        /// 計畫協辦機關
        /// </summary>
        public new List<ProjectAsstOrgAdjustModel> ProjectAsstOrg { get; set; }
    }
}
