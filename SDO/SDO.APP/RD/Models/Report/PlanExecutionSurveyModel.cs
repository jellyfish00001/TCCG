using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 表1 季別委託研究計畫列管表
    /// </summary>
    public class PlanExecutionSurveyModel
    {
        /// <summary>
        /// 委託機關
        /// </summary>
        public string OU_NAME { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PLAN_NO { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PLAN_NAME { get; set; }

        /// <summary>
        /// 研究期程起
        /// </summary>
        public string PLAN_START_DATE { get; set; }

        /// <summary>
        /// 研究期程迄
        /// </summary>
        public string PLAN_END_DATE { get; set; }

        /// <summary>
        /// 研究經費(千元)
        /// </summary>
        public decimal SUM_MONEY { get; set; }

        /// <summary>
        /// 研究單位
        /// </summary>
        public string ENTRUST_UNIT_NAME { get; set; }

        /// <summary>
        /// 研究員因
        /// </summary>
        public string PLAN_CAUSE { get; set; }

        /// <summary>
        /// 預期研究成果
        /// </summary>
        public string PLAN_EXPECTED { get; set; }

        /// <summary>
        /// 結案一年內參採情形
        /// </summary>
        public string SITUATION_DESC { get; set; }

        /// <summary>
        /// 計畫主持人
        /// </summary>
        public string RESEARCH_NAME { get; set; }

        /// <summary>
        /// 結案一年內之研究計畫成果整體評估
        /// </summary>
        public string SITUATION_TYPE { get; set; }

        /// <summary>
        /// 填表日期
        /// </summary>
        public string CRT_DATE { get; set; }

    }
}
