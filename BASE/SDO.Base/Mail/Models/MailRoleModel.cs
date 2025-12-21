namespace SDO.Models
{
    public class MailRoleModel
    {
        /// <summary>
        /// 角色代碼
        /// </summary>
        public string ROLE_ID { get; set; }

        /// <summary>
        /// 角色名稱
        /// </summary>
        public string ROLE_NAME { get; set; }

        /// <summary>
        /// 是否已刪除
        /// </summary>
        public bool DEL_FLG { get; set; }
    }
}