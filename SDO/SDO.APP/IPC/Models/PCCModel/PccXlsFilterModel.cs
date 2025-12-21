using SDO.APP.IPC.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class PccXlsFilterModel
    {
        public string DSNO { get; set; }
        /// <summary>
        /// 同步日期
        /// </summary>
        public DateTime? SYNC_DATE { get; set; }
        /// <summary>
        /// 標案編號
        /// </summary>
        public string PCC_PROJECT_NO { get; set; }
        /// <summary>
        /// 標案名稱
        /// </summary>
        public string PCC_PROJECT_NAME { get; set; }
    }
}
