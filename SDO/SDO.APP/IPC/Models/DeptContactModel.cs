
namespace SDO.Models
{
    /// <summary>
    /// 機關窗口Model
    /// </summary>
    public class DeptContactModel
    {
        /// <summary>
        /// 流水編號
        /// </summary>
        public int DC_ID { get; set; }
        /// <summary>
        /// 機關代碼
        /// </summary>
        public string ORGAN { get; set; }
        /// <summary>
        /// 來源 1:SC 2:自訂
        /// </summary>
        public int SOURCE { get; set; }
        /// <summary>
        /// 人員名稱/ID
        /// </summary>
        public string CONTACT { get; set; }
        /// <summary>
        /// EMAIL
        /// </summary>
        public string EMAIL { get; set; }
    }
}
