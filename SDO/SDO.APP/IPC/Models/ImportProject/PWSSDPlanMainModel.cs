namespace SDO.Models
{
    public class PWSSDPlanMainModel
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
        /// 主管機關代碼
        /// </summary>
        public string OU_ID { get; set; }

        public string PlanDateType { get; set; }
        /// <summary>
        /// 執行機關 (提報機關的一級機關)
        /// </summary>
        public string ExecOrgId { get; set; }
        /// <summary>
        /// 提報機關 (可能為1級或2級)
        /// </summary>
        public string CreateOrgOuId { get; set; }

        public string ExplainNecessity { get; set; }

        public string ExplanBasicInfo { get; set; }
        public string CreatedUserId { get; set; }
    }
}
