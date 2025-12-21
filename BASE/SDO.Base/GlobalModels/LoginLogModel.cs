using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class LoginLogModel : DbEditor
    {
        /// <summary>
        /// 公司統編
        /// </summary>
        public string COMP_ID { get; set; }
        /// <summary>
        /// 公司名稱
        /// </summary>
        public string COMP_NAME { get; set; }
        /// <summary>
        /// 帳號
        /// </summary>
        public string USER_ID { get; set; }

        /// <summary>
        /// IP位置
        /// </summary>
        public string USER_IP { get; set; }

        public DateTime LOG_TIME { get; set; }

        public string LOG_AP { get; set; }
        /// <summary>
        /// 登入訊息ID
        /// </summary>
        public string MSG_ID { get; set; }
        /// <summary>
        /// 登入訊息內容
        /// </summary>
        public string MSG_CONTENT { get; set; }
        /// <summary>
        /// 登入詳情
        /// </summary>
        public string MSG_DETAIL { get; set; }
    }
}
