using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class PccmXlsGridModel
    {
        /// <summary>
        /// Header Dict
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// 資料
        /// </summary>
        public List<Dictionary<string, string>> Contents { get; set; }
    }
}
