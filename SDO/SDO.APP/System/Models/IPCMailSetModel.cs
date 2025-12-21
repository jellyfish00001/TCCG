using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class IPCMailSetModel: MailSetModel
    {
        /// <summary>
        /// 郵件設定類型
        /// </summary>
        public string MAIL_TYPE { get; set; }

        /// <summary>
        /// 備註(郵件收件者)
        /// </summary>
        public string MAIL_MEMO { get; set; }
    }
}
