using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 表1.6 重大施政計畫先期審查表
    /// </summary>
    public class RPTProjectReviewList : FundingExecutionModel
    {

        /// <summary>
        /// 計畫編號
        /// </summary>
        //public string PLANNO { get; set; }
        /// <summary>
        /// 計畫年度
        /// </summary>
        public string PLANYEAR { get; set; }
        /// <summary>
        /// 局處
        /// </summary>
        public string OU_ID { get; set; }
        /// <summary>
        /// 局處
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
        /// 提報單位
        /// </summary>
        public string UNITOUNAME { get; set; }
        /// <summary>
        /// 提報機關ID
        /// </summary>
        public string CREATEORGOUID { get; set; }
        /// <summary>
        /// 提報單位
        /// </summary>
        public string ORGOUNAME { get; set; }
        /// <summary>
        /// 計畫性質
        /// </summary>
        public string PLANDATETYPE { get; set; }
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
        /// 計畫總經費
        /// </summary>
        public int PLANTOTMONEY { get; set; }
        /// <summary>
        /// 是否屬市長政策或指示項目
        /// </summary>
        public string PLANORGINYN { get; set; }
        /// <summary>
        /// 涉及建築裝修工程
        /// </summary>
        public string PLANCONTENTENGINE { get; set; }
        /// <summary>
        /// 涉及用地取得
        /// </summary>
        public string PLANCONTENTLAND { get; set; }
        /// <summary>
        /// 涉及資訊費用
        /// </summary>
        public string PLANCONTENTINFO { get; set; }
        /// <summary>
        /// 經費需求細項 
        /// </summary>
        public string FUNDDESC { get; set; }
        /// <summary>
        /// 金額(千元) 
        /// </summary>
        public int FUNDTOT { get; set; }
        /// <summary>
        /// 計算方式或說明 
        /// </summary>
        public string CALCULATIONDESC { get; set; }
        /// <summary>
        /// 二、年度 
        /// </summary>
        public int EXEYEAR { get; set; }
        /// <summary>
        /// 法定預算數 (千元)
        /// </summary>
        public int PUBLIC_MONEY { get; set; }
        /// <summary>
        /// 預算成長幅度 (％)
        /// </summary>
        public float GROWRATIO { get; set; }
        /// <summary>
        /// 預算執行數(千元)
        /// </summary>
        public int EXECOUNT { get; set; }
        /// <summary>
        /// 預算執行率(％)
        /// </summary>
        public float RATIO { get; set; }
        /// <summary>
        /// 預算執行說明
        /// </summary>
        public float EXEDESC { get; set; }
        /// <summary>
        /// 無歷年預算執行率
        /// </summary>
        public string NOBUDGETYN { get; set; }
        /// <summary>
        /// 計畫起始日期
        /// </summary>
        public string PLANSTARTDATE { get; set; }
        /// <summary>
        /// 計畫結束日期
        /// </summary>
        public string PLANENDDATE { get; set; }


    }
}
