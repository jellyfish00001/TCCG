using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class ThreeYearPlanModel : DbEditor
    {
        /// <summary>
        /// 計畫序號
        /// </summary>
        public int PLANID { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PLANNO { get; set; }

        /// <summary>
        /// 近三年計畫編號(序號)
        /// </summary>
        public int THREEYEARID { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PLANNAME { get; set; }

        /// <summary>
        /// 研究年度
        /// </summary>
        public int STUDYYEAR { get; set; }

        /// <summary>
        /// 研究經費
        /// </summary>
        public int BUDGET { get; set; }

        /// <summary>
        /// 參採情形    1.採行、2.參採、3.存查
        /// </summary>
        public int SITUATIONTYPE { get; set; }

        // <summary>
        /// 參採情形說明
        /// </summary>
        public string SITUATIONDESC { get; set; }

    }
}
