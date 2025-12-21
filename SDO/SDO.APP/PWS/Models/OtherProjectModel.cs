using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class OtherProjectModel 
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PROJECT_NAME { get; set; }

        /// <summary>
        /// 主管機關編號
        /// </summary>
        public string MASTER_ORGAN_C { get; set; }

        /// <summary>
        /// 主觀機關名稱
        /// </summary>
        public string MASTER_ORGAN_NAME { get; set; }

        /// <summary>
        /// 執行機關編號
        /// </summary>
        public string EXEC_ORGAN_C { get; set; }

        /// <summary>
        /// 執行機關名稱
        /// </summary>
        public string EXEC_ORGAN_NAME { get; set; }

        /// <summary>
        /// 計畫內容
        /// </summary>
        public string ALL_JOB { get; set; }

    }
}

