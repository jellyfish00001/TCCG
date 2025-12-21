using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫建設類別
    /// </summary>
    public class ProjectBuildKindModel 
    {
        /// <summary>
        /// 流水編號
        /// </summary>
        public int BUILD_ID { get; set; }

        /// <summary>
        /// 列管編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string BUILD_KIND_TYPE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string BUILD_KIND { get; set; }
    }
}
