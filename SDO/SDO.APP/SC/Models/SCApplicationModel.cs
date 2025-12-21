using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class SCApplicationModel
    {
        /// <summary>
        /// 先期計畫數量
        /// </summary>
        public int PwssdCnt { get; set; }

        /// <summary>
        /// 研究發展最大年度
        /// </summary>
        public string RdMaxYear { get; set; }

        /// <summary>
        /// 委託研究數量
        /// </summary>
        public int RdRpmCnt { get; set; }

        /// <summary>
        /// 創新提案數量
        /// </summary>
        public int RdWipCnt { get; set; }
    }
}
