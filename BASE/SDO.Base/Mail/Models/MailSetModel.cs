using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
namespace SDO.Models
{
    public class MailSetModel : DbEditor
    {
        /// <summary>
        /// 郵件範本代碼
        /// </summary>
        public virtual string MAIL_ID { get; set; }

        /// <summary>
        /// 郵件範本名稱
        /// </summary>
        public virtual string MAIL_NAME { get; set; }

        /// <summary>
        /// 郵件範本主旨
        /// </summary>
        public string MAIL_SUBJECT { get; set; }

        /// <summary>
        /// 郵件範本內文
        /// </summary>
        public string MAIL_CONTENT { get; set; }

        /// <summary>
        /// 是否已刪除
        /// </summary>
        public bool DEL_FLG { get; set; }

        /// <summary>
        /// 計畫申請編號
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 登入者ID對應的Role
        /// </summary>
        public string MAP_ROLE_USER { get; set; }
    }
}