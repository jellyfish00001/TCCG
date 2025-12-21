using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 標案系統執行進度GridModel
    /// </summary>
    public class PCCExeProgressGridModel
    {
        /// <summary>
        /// 執行進度 Header Dict
        /// </summary>
        public Dictionary<string, string> ProgressHeaders { get; set; }

        /// <summary>
        /// 執行進度 資料
        /// </summary>
        public List<Dictionary<string, string>> ProgressDatas { get; set; }

        /// <summary>
        /// 落後原因 Header Dict
        /// </summary>
        public Dictionary<string, string> DelayHeaders { get; set; }

        /// <summary>
        /// 落後原因 資料
        /// </summary>
        public List<Dictionary<string, string>> DelayDatas { get; set; }
    }
}
