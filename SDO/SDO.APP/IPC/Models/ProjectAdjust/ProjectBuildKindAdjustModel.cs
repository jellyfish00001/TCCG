using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.IPC.Models.ProjectAdjust
{
    public class ProjectBuildKindAdjustModel : ProjectBuildKindModel
    {
        /// <summary>
        /// 調整檔流水號
        /// </summary>
        public int PROJ_ADJ_ID { get; set; }
    }
}
