using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 密碼變更
    /// </summary>
    public class SCRefreshModel
    {
        /// <summary>
        /// 帳號
        /// </summary>
        public string USER_ID { get; set; }

        /// <summary>
        /// 原密碼
        /// </summary>
        public string ORIGINAL_USER_PD { get; set; }

        /// <summary>
        /// 新密碼
        /// </summary>
        public string USER_PD { get; set; }
    }
}
