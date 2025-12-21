using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// SMTP設定
    /// </summary>  
    public class SMTPSettingModel
    {
        /// <summary>
        /// smtp server 位址
        /// </summary>
        public string serverAddr { get; set; }
        /// <summary>
        /// port
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// smtp登入帳號
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// smtp登入密碼
        /// </summary>
        public string PD { get; set; }
        /// <summary>
        /// 預設寄件信箱
        /// </summary>
        public string Sender { get; set; }
        /// <summary>
        /// 預設寄件者名稱
        /// </summary>
        public string SenderName { get; set; }
    }
}
