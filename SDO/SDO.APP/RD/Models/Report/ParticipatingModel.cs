using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 表 4 委託研究計畫結案情形總表
    /// </summary>
    public class ParticipatingModel
    {
        /// <summary>
        /// 年度
        /// </summary>
        public string PLAN_YEAR { get; set; }  

        /// <summary>
        /// 委託機關
        /// </summary>
        public string OU_NAME { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PLAN_NAME { get; set; }

        /// <summary>
        /// 研究期程
        /// </summary>
        public string PLAN_DATE { get; set; }

        /// <summary>
        /// 受委託人
        /// </summary>
        public string ENTRUST_UNIT_NAME { get; set; }

        /// <summary>
        /// 研究經費(千元)
        /// </summary>
        public decimal SUM_MONEY { get; set; }

        /// <summary>
        /// 研究主持人
        /// </summary>
        public string RESEARCH_NAME { get; set; }

        /// <summary>
        /// 研究建議處理情形
        /// </summary>
        public string SITUATION_TYPE { get; set; }

        /// <summary>
        /// 採行情形簡述
        /// </summary>
        public string SITUATION_DESC { get; set; }

        public string PLAN_START_DATE { get; set; }

        public string PLAN_END_DATE { get; set; }


    }
}
