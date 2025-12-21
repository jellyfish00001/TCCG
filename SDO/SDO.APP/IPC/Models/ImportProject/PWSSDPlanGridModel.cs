namespace SDO.Models
{
    public class PWSSDPlanGridModel
    {
        /// <summary>
        ///  計畫Id
        /// </summary>
        public int PlanId { get; set; }
        /// <summary>
        ///  計畫年度
        /// </summary>
        public string PlanYear { get; set; }
        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PlanName { get; set; }
        /// <summary>
        /// 機關
        /// </summary>
        public string OU_NAME { get; set; }
        /// <summary>
        /// 計畫總經費
        /// </summary>
        public double PlanTotMoney { get; set; }
        /// <summary>
        ///  計畫總經費(顯示)
        /// </summary>
        public string PlanTotMoneyFormat
        {
            get { return string.Format("{0:N0}", this.PlanTotMoney); }
        }
        /// <summary>
        /// 計畫期程
        /// </summary>
        public string PlanDate { get; set; }
        /// <summary>
        /// 審查狀態
        /// </summary>
        public string SendStatus { get; set; }
        /// <summary>
        /// 匯入狀態
        /// </summary>
        public bool IsImport { get; set; }
        /// <summary>
        /// 執行單位 => 提報單位(提報機關)
        /// </summary>
        public string ExecUnitName { get; set; }
        /// <summary>
        /// 執行機關(提報機關的一級機關)
        /// </summary>
        public string ExecOrgName { get; set; }
    }
}
