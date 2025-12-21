using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 透過郵件範本寄信所需參數物件
    /// </summary>
    public class MailTemplateSendModel<T>
    {
        /// <summary>
        /// 範本
        /// </summary>
        public string TemplateId { get; set; }
        /// <summary>
        /// 範本替換參數
        /// </summary>
        public T TemplatePara { get; set; }
        /// <summary>
        /// 收件人資料
        /// </summary>
        public List<RecipientModel> MailAddrs  { get; set; }
        /// <summary>
        /// 附件
        /// </summary>
        public List<Attachment> Attachments { get; set; }
    }
}
