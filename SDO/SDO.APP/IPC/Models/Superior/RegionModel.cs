using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class RegionModel
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 辦理地點
        /// </summary>
        public string TOWN_C { get; set; }

        /// <summary>
        /// 進度落差
        /// </summary>
        public int PRG_OFFSET { get; set; }
    }
}
