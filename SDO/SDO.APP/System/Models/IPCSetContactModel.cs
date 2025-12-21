using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 機關窗口維護
    /// </summary>
    public class IPCSetContactModel
    {
        /// <summary>
        /// 機關代碼
        /// </summary>
        public string ORGAN { get; set; }

        /// <summary>
        /// 機關名稱
        /// </summary>
        public string ORGAN_NAME { get; set; }

        /// <summary>
        /// 機關窗口維護資料
        /// </summary>
        public List<IPCDeptContactModel> DeptContact { get; set; }
    }
}
