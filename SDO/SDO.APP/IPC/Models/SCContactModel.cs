using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDO.Models;

namespace SDO.Models
{
    /// <summary>
    /// SC人員信箱Model
    /// </summary>
    public class SCContactModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public string USR_ID { get; set; }
        /// <summary>
        /// 人員名稱
        /// </summary>
        public string USR_NAME { get; set; }
        /// <summary>
        /// 信箱
        /// </summary>
        public string USR_EMAIL { get; set; }

    }
}
