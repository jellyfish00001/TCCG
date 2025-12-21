using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 表2.3.4.5重大彙整 表8.9 委託審查 
    /// </summary>
    public class RPTBudgeReviewModel : DbEditor
    {

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PLANNO { get; set; }
        /// <summary>
        /// 主辦機關ID(主管機關)
        /// </summary>
        public string OU_ID { get; set; }
        /// <summary>
        /// 主辦機關名稱(主管機關)
        /// </summary>
        public string OU_NAME { get; set; }
        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PLANNAME { get; set; }
        /// <summary>
        /// 優先順序
        /// </summary>
        public int PLANORDERNUMBER { get; set; }
        /// <summary>
        /// 提報單位ID
        /// </summary>
        public string CREATEUNITOUID { get; set; }
        /// <summary>
        /// 提報單位名稱
        /// </summary>
        public string UNITOUNAME { get; set; }
        /// <summary>
        /// 提報機關ID
        /// </summary>
        public string CREATEORGOUID { get; set; }
        /// <summary>
        /// 提報機關名稱
        /// </summary>
        public string ORGOUNAME { get; set; }
        /// <summary>
        /// 基金編號
        /// </summary>
        public string FUNDNO { get; set; }
        /// <summary>
        /// 基金名稱
        /// </summary>
        public string FUNDNAME { get; set; }
        /// <summary>
        /// 跨年度預算來源 公共:公務 基金:基金
        /// </summary>
        public string SOURCEKIND { get; set; }
        /// <summary>
        /// 跨年度中央補助款
        /// </summary>
        public int BUDGETCENTRAL { get; set; }
        /// <summary>
        /// 跨年度地方自籌款
        /// </summary>
        public int BUDGETLOCAL { get; set; }
        /// <summary>
        /// 計畫為 公務預算or基金預算
        /// </summary>
        public string BUDGETTYPE { get; set; }
        /// <summary>
        /// 公務預算
        /// </summary>
        public int PUBLICMONEY { get; set; }
        /// <summary>
        /// 基金預算
        /// </summary>
        public int FUNDMONEY { get; set; }
        /// <summary>
        /// 跨年度公務預算
        /// </summary>
        public int CROSS_PUBLICMONEY { get; set; }
        /// <summary>
        /// 跨年度基金預算
        /// </summary>
        public int CROSS_FUNDMONEY { get; set; }
        /// <summary>
        /// 公務 建議核列
        /// </summary>
        public int PUBLIC1 { get; set; }
        /// <summary>
        /// 基金 建議核列
        /// </summary>
        public int FUND1 { get; set; }
        /// <summary>
        /// 公務 不建議核列
        /// </summary>
        public int PUBLIC2 { get; set; }
        /// <summary>
        /// 基金 不建議核列
        /// </summary>
        public int FUND2 { get; set; }
        /// <summary>
        /// 專案小組審查意見
        /// </summary>
        public string ADVIEWDESC { get; set; }

        /// <summary>
        /// 計畫總數
        /// </summary>
        public int PLANCOUNT { get; set; }
    }
}
