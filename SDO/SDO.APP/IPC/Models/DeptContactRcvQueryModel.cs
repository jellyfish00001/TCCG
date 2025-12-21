
namespace SDO.Models
{
    /// <summary>
    /// 查詢機關窗口收件人資料Model
    /// </summary>
    public class DeptContactRcvQueryModel
    {
        /// <summary>
        /// 人員名稱/ID
        /// </summary>
        public string OrgId { get; set; }
        /// <summary>
        /// 郵寄類型
        /// 0 : 寄件者
        /// 1 : 收件者
        /// 2 : 副本
        /// 3 : 密件副本
        /// </summary>
        public string MailType { get; set; }
    }
}
