using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 報表列印model
    /// </summary>
    public class InnStatisticsModel
    {
        /// <summary>
        /// 報表編號
        /// </summary>
        public string RPT_ID { get; set; }
        /// <summary>
        /// 報表名稱
        /// </summary>
        public string STATISTICS_NAME { get; set; }
        /// <summary>
        /// 計畫年度
        /// </summary>
        public string INN_YEAR { get; set; }

        public List<ProjectTitleModel> projectTitles { get; set; }

    }
}
