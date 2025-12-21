using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ProjectCategoryDelayModel
    {
        /// <summary>
        /// 計畫類別 (規劃中、施工、驗收)
        /// </summary>
        public string PROJECT_CATEGORY { get; set; }
        /// <summary>
        /// 落後件數
        /// </summary>
        public int DelayCount { get; set; }
    }
}
