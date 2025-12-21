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
    public class ZipFileModel
    {
        public int FILE_ID { get; set; }
        /// <summary>
        /// 報告類型名稱
        /// </summary>
        public string FILE_NAME { get; set; }
        /// <summary>
        /// 檔案路徑
        /// </summary>
        public string FILE_PATH { get; set; }
        /// <summary>
        /// 檔案類型
        /// </summary>
        public string FILE_KIND { get; set;}
        /// <summary>
        /// 檔案完整路徑 含檔名
        /// </summary>
        public string FILE_FULL_PATH { get; set; }
    }
}
