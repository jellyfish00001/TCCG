using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫相關歷程檔 Model
    /// </summary>
    public class ProjectHisModel : DbEditor
    {
        /// <summary>
        /// 歷程黨Id
        /// </summary>
        public int HIS_ID { get; set; }
        /// <summary>
        /// 異動紀錄ID
        /// </summary>
        public int LOG_ID { get; set; }
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }
    }
}
