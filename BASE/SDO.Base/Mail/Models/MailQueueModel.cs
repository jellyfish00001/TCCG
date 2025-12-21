using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace SDO.Models
{
    public class MailQueueModel
    {
        /// <summary>
        /// 郵件排程代碼
        /// </summary>
        public string QUEUE_ID { get; set; }

        /// <summary>
        /// 郵件排程主旨
        /// </summary>
        [Required]
        [Display(Name = "MailQueue_MAIL_SUBJECT", ResourceType = typeof(i18N.Label))]
        public string MAIL_SUBJECT { get; set; }

        /// <summary>
        /// 郵件排程內文
        /// </summary>
        [Required]
        [Display(Name = "MailSet_MAIL_CONTENT", ResourceType = typeof(i18N.Label))]
        public string MAIL_CONTENT { get; set; }

        /// <summary>
        /// 郵件排程寄出標記
        /// </summary>
        public int SEND_FLG { get; set; }

        /// <summary>
        /// 郵件排程建立時間
        /// </summary>
        public string CRT_DATE { get; set; }

        /// <summary>
        /// 郵件排程寄出時間
        /// </summary>
        public string SEND_TIME { get; set; }
    }
}
