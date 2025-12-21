using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 表7 委託研究審查計畫 (Word)
    /// </summary>
    public class RPTProjectEntListModel : FundingExecutionModel
    {
        /// <summary>
        /// 計畫年度
        /// </summary>
        public int PLANYEAR { get; set; }  
        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PLANNAME { get; set; }
        /// <summary>
        /// 委託機關名稱
        /// </summary>
        public string OU_NAME { get; set; }
        /// <summary>
        /// 是否為勞務委託
        /// </summary>
        public string LABORYN { get; set; }
        /// <summary>
        /// 起始日期
        /// </summary>
        public DateTime PLANSTARTDATE { get; set; }
        /// <summary>
        /// 結束日期
        /// </summary>
        public DateTime PLANENDDATE { get; set; }
        /// <summary>
        /// 決標年月
        /// </summary>
        public DateTime AWARDYM { get; set; }
        /// <summary>
        /// 期中報告年月
        /// </summary>
        public DateTime MIDREPORTYM { get; set; }
        /// <summary>
        /// 期末報告年月
        /// </summary>
        public DateTime FINAKREPORTYM { get; set; }
        /// <summary>
        /// 結案年月
        /// </summary>
        public DateTime CLOSEYM { get; set; }
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
        /// 計畫性質
        /// </summary>
        public string PLANDATETYPE { get; set; }
        /// <summary>
        /// 公務預算
        /// </summary>
        public int PUBLICMONEY { get; set; }
        /// <summary>
        /// 基金預算
        /// </summary>
        public int FUNDMONEY { get; set; }
        /// <summary>
        /// 中央預算
        /// </summary>
        public int CENTERMONEY { get; set; }
        /// <summary>
        /// 其他預算
        /// </summary>
        public int OTHERMONEY { get; set; }
        /// <summary>
        /// 其他預算說名
        /// </summary>
        public string OTHERDESC { get; set; }
        /// <summary>
        /// 中央算核定否
        /// </summary>
        public string APPROVEDYN { get; set; }
        /// <summary>
        /// 核定文號
        /// </summary>
        public string APPROVEDNUMBER { get; set; }
        /// <summary>
        /// 是否報請核定
        /// </summary>
        public string APPLYAPPROVEDYN { get; set; }
        /// <summary>
        /// 計劃組經費
        /// </summary>
        public int PLANTOTMONEY { get; set; }
        /// <summary>
        /// 研究原因及目的
        /// </summary>
        public string PLANCAUSE { get; set; }
        /// <summary>
        /// 預期研究成果
        /// </summary>
        public string PLANEXPECTED { get; set; }
        /// <summary>
        /// 之前年度經費
        /// </summary>
        public int BYMoney { get; set; }
        /// <summary>
        /// 年度經費
        /// </summary>
        public int NYMoney { get; set; }
        /// <summary>
        /// 之後年度經費
        /// </summary>
        public int AYMoney { get; set; }
    }
}
