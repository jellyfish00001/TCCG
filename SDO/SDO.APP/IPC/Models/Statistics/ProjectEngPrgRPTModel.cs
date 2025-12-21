using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 表11 : 工程進度所需資料
    /// </summary>
    public class ProjectEngPrgRPTModel : ProjectEngineeringProgressModel
    {
        /// <summary>
        /// 執行機關
        /// </summary>
        public string EXEC_ORGAN_C { get; set; }

        /// <summary>
        /// 進度差異
        /// </summary>
        public decimal? PRG_DIFF
        {
            get
            {
                if (IPC_RES_PRG == null)
                    return null;

                return IPC_ACT_PRG  - IPC_RES_PRG.Value;
            }
        }
    }
}
