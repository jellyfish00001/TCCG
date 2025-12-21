namespace SDO.Models
{
    public class MailRecipientModel
    {
        /// <summary>
        /// 郵寄ID
        /// </summary>
        public string MAIL_ID { get; set; }

        /// <summary>
        /// 郵寄類型
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
