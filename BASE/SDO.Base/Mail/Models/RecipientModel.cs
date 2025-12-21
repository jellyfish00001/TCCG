using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class RecipientModel : DbEditor
    {
        /// <summary>
        /// 郵件範本代碼
        /// </summary>
        [Required]
        public virtual string MAIL_ID { get; set; }
        /// <summary>
        /// 郵寄類型
        /// 0 : 寄件者
        /// 1 : 收件者
        /// 2 : 副本
        /// 3 : 密件副本
        /// </summary>
        public string MAIL_TYPE { get; set; }

        /// <summary>
        /// 郵件角色代碼
        /// </summary>
        public string MAIL_ROLE { get; set; }

        /// <summary>
        /// 自訂使用者信箱
        /// </summary>
        public string MAIL_ADDRESS { get; set; }

        /// <summary>
        /// 自訂使用者名稱
        /// </summary>
        public string MAIL_TITLE { get; set; }
    }
}
