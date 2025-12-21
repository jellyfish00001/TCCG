using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 更改送審狀態 Model
    /// </summary>
    public class RDAuditStatusModel : RDAuditModel
    {
        /// <summary>
        /// 各章節章節資料表名
        /// </summary>
        public string TABLE { get; set; }

        /// <summary>
        /// 資料表的審核狀態欄位名
        /// </summary>
        public string STATUS_FIELD { get; set; }

        /// <summary>
        /// 資料表的序號(SEQ 或 EXTENSION_ID)欄位名
        /// </summary>
        public string ID_FIELD { get; set; }

        /// <summary>
        /// 送審狀踏
        /// </summary>
        /// <remarks>
        /// 送審狀態 STATUS
        /// 1:未送審、2:待審核、3:取消申請、4:審核通過、5:審核不通過
        /// </remarks>
        public string STATUS { get; set; }
    }
}
