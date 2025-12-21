using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class MailSetMdfModel : MailSetModel
    {
        /// <summary>
        /// 郵件範本代碼
        /// </summary>
        [Required]
        public override string MAIL_ID { get; set; }

        /// <summary>
        /// 郵件範本名稱
        /// </summary>
        [Required]
        public override string MAIL_NAME { get; set; }
    }
}
