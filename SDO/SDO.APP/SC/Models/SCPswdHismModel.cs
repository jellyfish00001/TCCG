using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 密碼變更記錄檔
    /// </summary>
    public class SCPswdHismModel
    {
        /// <summary>
        /// 使用者代號
        /// </summary>
        public string USR_ID { get; set; }

        /// <summary>
        /// 密碼
        /// </summary>
        public string PASSWORD { get; set; }

        /// <summary>
        /// 變更日期
        /// </summary>
        public DateTime CHANPSWD_DATE { get; set; }
    }
}
