using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Base.Utils.Models
{
    /// <summary>
    /// 上傳檔案資訊 Model
    /// </summary>
    public class SaveFileModel
    {
        /// <summary>
        /// 檔案存檔位置，不含檔名
        /// </summary>
        public string SavePath { get; set; }
        /// <summary>
        /// 新檔案名稱 Null為不修改
        /// </summary>
        public string NewFileName { get; set; }
        /// <summary>
        /// 要刪除的檔案名稱
        /// </summary>
        public string DeleteFileName { get; set; }
    }
}
