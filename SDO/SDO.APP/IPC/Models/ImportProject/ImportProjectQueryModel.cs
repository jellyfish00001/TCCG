namespace SDO.Models
{
    public class ImportProjectQueryModel
    {
        /// <summary>
        /// 年度
        /// </summary>
        public string planYear { get; set; }
        /// <summary>
        /// 機關
        /// </summary>
        public string organ { get; set; }
        /// <summary>
        /// 審核狀態
        /// </summary>
        public string isSend { get; set; }
        /// <summary>
        /// 匯入狀態
        /// </summary>
        public string isImport { get; set; }
    }
}
