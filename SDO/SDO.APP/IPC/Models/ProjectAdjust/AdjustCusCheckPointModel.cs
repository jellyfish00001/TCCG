using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.IPC.Models.ProjectAdjust
{
    public class AdjustCusCheckPointModel : ProjectCusCheckpointModel
    {
        /// <summary>
        /// 調整檔流水號
        /// </summary>
        public int PROJ_ADJ_ID { get; set; }
        /// <summary>
        /// 原最後一個預定完成日期
        /// </summary>
        public DateTime? ORI_ESTIMATED_ENDDATE { get; set; }
    }
}
