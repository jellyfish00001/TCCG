using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 機關窗口維護資料
    /// </summary>
    public class IPCDeptContactModel : DbEditor
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public int DC_ID { get; set; }

        /// <summary>
        /// 機關代碼
        /// </summary>
        public string ORGAN { get; set; }

        /// <summary>
        /// 來源 1：SC  2：自訂
        /// </summary>
        public int SOURCE { get; set; }

        /// <summary>
        /// 機關承辦人員(SC人員存帳號、自訂人員存姓名)
        /// </summary>
        public string CONTACT { get; set; }

        /// <summary>
        /// 機關承辦人員姓名(SC人員姓名)
        /// </summary>
        public string CONTACT_NAME { get; set; }

        /// <summary>
        /// 連絡電話
        /// </summary>
        public string TEL { get; set; }

        /// <summary>
        /// 聯絡信箱
        /// </summary>
        public string EMAIL { get; set; }
    }
}
