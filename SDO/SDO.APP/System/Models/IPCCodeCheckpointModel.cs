using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 執行方式代碼
    /// </summary>
    public class IPCCodeCheckpointModel : DbEditor
    {
        /// <summary>
        /// 執行方式編號
        /// </summary>
        public int CHECKPOINT_CLASS_ID { get; set; }

        /// <summary>
        /// 執行方式名稱
        /// </summary>
        public string CHECKPOINT_CLASS { get; set; }

        /// <summary>
        /// 執行方式類別
        /// </summary>
        public string CP_KIND { get; set; }

        /// <summary>
        /// 執行方式類別中文
        /// </summary>
        public string CP_KIND_DESC { get; set; }

        /// <summary>
        /// 是否為基本資料
        /// </summary>
        public bool IS_BASIC { get; set; }

        /// <summary>
        /// 刪除註記
        /// </summary>
        public bool DEL_FLG { get; set; }
    }
}
