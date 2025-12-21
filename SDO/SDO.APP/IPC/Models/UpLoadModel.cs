using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class UpLoadModel
    {
        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 檔案類型
        /// </summary>
        public string FILE_UP_SOURCE { get; set; }

        /// <summary>
        /// 檔案來源
        /// </summary>
        public string? FILE_KIND { get; set; } = "";

        /// <summary>
        /// DB來源
        /// </summary>
        public int DBKey { get; set; } = 0;
    }
}
