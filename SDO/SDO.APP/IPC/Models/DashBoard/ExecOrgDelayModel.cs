using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 機關落後Model
    /// </summary>
    public class ExecOrgDelayModel
    {
        /// <summary>
        /// 執行機關代碼
        /// </summary>
        public string  EXEC_ORGAN_C { get; set; }
        /// <summary>
        /// 落後比率
        /// </summary>
        public decimal  DELAY_RATE { get; set; }
    }
}
