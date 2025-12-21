using SDO.APP.IPC.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 送出功能 驗證錯誤資訊model
    /// </summary>
    public class ProjectSubmitErrorModel
    {
        /// <summary>
        /// 錯誤章節名稱
        /// </summary>
        public string Chapter { get; set; }

        /// <summary>
        /// 錯誤章節Id
        /// </summary>
        public string ChapterId { get; set; }
        /// <summary>
        /// 章節前端程式路徑
        /// </summary>
        public string ChapterUrl { get; set; }

        /// <summary>
        /// 錯誤欄位名稱
        /// </summary>
        public string ErrMsg { get; set; }
    }
}
