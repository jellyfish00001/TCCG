using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 報表查詢Model
    /// </summary>
    public class PWSReportModel : DbEditor
    {
        /// <summary>
        /// 報表編號
        /// </summary>
        public string RPT_ID { get; set; }
        /// <summary>
        /// 報表名稱
        /// </summary>
        public string STATISTICS_NAME { get; set; }
        /// <summary>
        /// 計畫年度
        /// </summary>
        public string PWS_YEAR { get; set; }
        /// <summary>
        /// 公務預算or基金預算
        /// </summary>
        public string BUDGETTYPE { get; set; }
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PLANNO { get; set; }
        /// <summary>
        /// 主辦機關ID
        /// </summary>
        public string? OU_ID { get; set; }
        /// <summary>
        /// 主辦機關ID
        /// </summary>
        public string? OU_NAME { get; set; }
        /// <summary>
        /// 計畫類別
        /// </summary>
        public string PLANKIND { get; set; }
        /// <summary>
        /// 計畫送出
        /// </summary>
        public int? IS_SEND { get; set; }
        /// <summary>
        /// 優先順序
        /// </summary>
        public int? SEND_STATUS { get; set; }
        /// <summary>
        /// 專案小組審核
        /// </summary>
        public int? AUDIT_STATUS { get; set; }
        /// <summary>
        /// 基金主管機關ID
        /// </summary>
        public string? FUND_OU_ID { get; set; }
        /// <summary>
        /// 基金主管機關
        /// </summary>
        public string? FUND_OU_NAME { get; set; }
        /// <summary>
        /// 提報單位ID
        /// </summary>
        public string? CREATEUNITOUID { get; set; }
        /// <summary>
        /// 提報單位名稱
        /// </summary>
        public string? CREATEUNITNAME { get; set; }
        /// <summary>
        /// 計畫NO(為計畫勾選匯出)
        /// </summary>
        public List<string> PLANNOList { get; set; }
    }
}
