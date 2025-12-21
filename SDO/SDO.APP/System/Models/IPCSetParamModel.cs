using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 檔案資訊Model
    /// </summary>
    public class IPCSetParamModel : SetParamModel
    {
        /// <summary>
        /// 排序
        /// </summary>
        public int SORT_ORDER { get; set; }

        /// <summary>
        /// 舊的 setType
        /// </summary>
        public string OLD_SET_TYPE { get; set; }
    }
}
