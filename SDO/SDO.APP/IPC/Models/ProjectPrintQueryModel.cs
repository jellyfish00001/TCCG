using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 預覽列印報表查詢
    /// </summary>
    public class ProjectPrintQueryModel
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 副檔名
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// 查詢類別
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 是否為管考功能
        /// </summary>
        public bool IsRdecFun { get; set; }

        /// <summary>
        /// 是否為差異比對
        /// </summary>
        public bool IsDiffCompare { get; set; }

        /// <summary>
        /// 歷程檔編號
        /// </summary>
        public List<int> LOG_ID { get; set; }
    }
}
