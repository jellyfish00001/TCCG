namespace SDO.Models
{
    public class SCAppModel
    {
        /// <summary>
        /// 使用者帳號
        /// </summary>
        public string USR_ID { get; set; }
        /// <summary>
        /// 可使用系統
        /// </summary>
        public string AP_ID { get; set; }
        /// <summary>
        /// 系統名稱
        /// </summary>
        public string AP_NAME { get; set; }
        /// <summary>
        /// 系統連結
        /// </summary>
        public string PRG_PATH { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public string AP_SORT_ORDER { get; set; }
        /// <summary>
        /// 顯示類型
        /// </summary>
        public string DISPLAY_TYPE { get; set; }
    }
}
