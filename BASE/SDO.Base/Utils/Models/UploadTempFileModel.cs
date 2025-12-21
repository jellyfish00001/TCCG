using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Base.Utils.Models
{
    /// <summary>
    /// 上傳暫存檔案 Model
    /// </summary>
    public class UploadTempFileModel
    {
        /// <summary>
        /// 處裡種類 1:新增 2:刪除
        /// </summary>
        public int EditType { get; set; }
        /// <summary>
        /// Uid 值
        /// </summary>
        public string Uid { get; set; }
        /// <summary>
        /// 檔案名稱
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 副檔名
        /// </summary>
        public string Extension { get; set; }
        /// <summary>
        /// 檔案ID
        /// </summary>
        public int FileId { get; set; }
    }
}
