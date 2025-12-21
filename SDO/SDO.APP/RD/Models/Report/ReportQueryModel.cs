using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RD.Models.Report
{
    public class ReportQueryModel
    {
        /// <summary>
        /// 報表名稱
        /// </summary>
        public string STATISTICS_NAME { get; set; }
        /// <summary>
        /// 報表 ID
        /// </summary>
        public int REPORT_ID { get; set; }

        /// <summary>
        /// 填報年度
        /// </summary>
        public string PLAN_YEAR { get; set; }

        /// <summary>
        /// 季別
        /// </summary>
        public string SEASON_TYPE { get; set; }

        /// <summary>
        /// 局處
        /// </summary>
        public string OU_ID { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PLAN_NO { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PLAN_NAME { get; set; }

        /// <summary>
        /// 研究成果整體評估
        /// </summary>
        public string SITUATION_TYPE { get; set; }

        /// <summary>
        /// 研究期程_起
        /// </summary>
        public string PLAN_START_DATE { get; set; }

        /// <summary>
        /// 研究期程_迄
        /// </summary>
        public string PLAN_END_DATE { get; set; }
    }
}
