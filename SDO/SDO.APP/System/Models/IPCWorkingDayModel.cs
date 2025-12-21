using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 工作日
    /// </summary>
    public class IPCWorkingDayModel : DbEditor
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime DATE { get; set; }

        /// <summary>
        /// 是否為上班日
        /// </summary>
        public bool IS_WORKING { get; set; }
    }
}
