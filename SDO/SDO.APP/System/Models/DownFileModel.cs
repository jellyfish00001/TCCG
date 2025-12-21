using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 檔案下載Model
    /// </summary>
    public class DownFileModel
    {
        /// <summary>
        /// 檔名
        /// </summary>
        public string FILE_NAME { get; set; }
        /// <summary>
        /// 副檔名
        /// </summary>
        public string file_extension { get; set; }
        /// <summary>
        /// 檔案路徑
        /// </summary>
        public string FILE_PATH { get; set; }
    }
}
