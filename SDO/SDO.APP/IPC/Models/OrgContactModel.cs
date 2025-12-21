
using System.Collections.Generic;

namespace SDO.Models
{
    /// <summary>
    /// 機關窗口聯繫資訊Model
    /// </summary>
    public class OrgContactModel
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public List<string> PROJECT_NO { get; set; }
        /// <summary>
        /// 執行機關代碼
        /// </summary>
        public string EXEC_ORGAN_C { get; set; }
        /// <summary>
        /// 聯繫人
        /// </summary>
        public List<ContactData> ContactData { get; set; }
    }

    /// <summary>
    /// 聯繫資訊
    /// </summary>
    public class ContactData
    {
        public string Contact { get; set; }
        public string Email { get; set; }
    }
}
