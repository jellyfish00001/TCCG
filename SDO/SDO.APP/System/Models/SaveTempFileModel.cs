using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 暫存檔儲存 Model
    /// </summary>
    public class SaveTempFileModel
    {
        /// <summary>
        /// 暫存檔案名稱(Guid+副檔名)
        /// </summary>
        public string TempFileName { get; set; }
        /// <summary>
        /// 正式路徑
        /// </summary>
        public string FormalPath { get; set; }
        /// <summary>
        /// 正式檔案名稱(含副檔名)
        /// </summary>
        public string FormalFileName { get; set; }
    }
}
