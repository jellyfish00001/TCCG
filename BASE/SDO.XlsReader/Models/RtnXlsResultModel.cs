using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.XlsReader.Models
{
    public class RtnXlsResultModel
    {
        /// <summary>
        /// 表頭
        /// </summary>
        public List<string> Headers { get; set; }

        /// <summary>
        /// 內容
        /// </summary>
        public List<Dictionary<string, object>> Contents { get; set; }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string ErrMsg { get; set; }
    }
}
