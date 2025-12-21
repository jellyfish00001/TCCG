using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class ReviewProjectModel : AuditTemplateModel
    {

        /// <summary>
        /// 計畫年度
        /// </summary>
        public string PLANYEAR { get; set; }

        /// <summary>
        /// 計畫狀態
        /// </summary>
        public bool IS_SEND { get; set; }

        /// <summary>
        /// 執行類別
        /// </summary>
        public string PLANKIND { get; set; }

        /// <summary>
        /// 建議核列:公務預算
        /// </summary>
        public int PUBLIC1 { get; set; }

        /// <summary>
        /// 不建議核列:公務預算
        /// </summary>
        public int PUBLIC2 { get; set; }

        /// <summary>
        /// 建議核列:基金預算
        /// </summary>
        public int FUND1 { get; set; }

        /// <summary>
        /// 不建議核列:基金預算
        /// </summary>
        public int FUND2 { get; set; }

        /// <summary>
        /// 專案小組意見
        /// </summary>
        public string ADVIEWDESC { get; set; }

        /// <summary>
        /// 優先順序
        /// </summary>
        public string PLANORDERNUMBER { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PLANNO { get; set; }

        /// <summary>
        /// 計畫總經費
        /// </summary>
        public int PLANTOTMONEY { get; set; }

        /// <summary>
        /// 計畫基金金額
        /// </summary>
        public int FUNDMONEY { get; set; }

        /// <summary>
        /// 關聯重大的計畫編號
        /// </summary>
        public string IPC_PROJECTNO { get; set; }

        /// <summary>
        /// 專案小組是否審查
        /// </summary>
        public int AUDIT_STATUS { get; set; }

        /// <summary>
        /// 計畫性值
        /// </summary>
        public string BUDGETTYPE { get; set; }

        /// <summary>
        /// 專案小組意見資料
        /// </summary>
        public List<AuditTemplateModel> AuditTemplateModels { get; set; }

    }
}
