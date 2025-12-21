using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class ThreeYearPlanIDModel
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
        /// 近三年相關研究計畫資料
        /// </summary>
        public List<ThreeYearPlanModel> ThreeYearPlanList { get; set; }
    }
}
