using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫異動記錄清單 Model
    /// </summary>
    public class ProjectLogListModel : ProjectBasicLogModel
    {
        /// <summary>
        /// 異動紀錄ID
        /// </summary>
        public int LOG_ID { get; set; }
        /// <summary>
        /// 異動人員
        /// </summary>
        public string LOG_USER { get; set; }
        /// <summary>
        /// 執行時間 DateTime
        /// </summary>
        public DateTime LOG_DATE { get; set; }
        /// <summary>
        /// 執行時間(顯示)
        /// </summary>
        public string LOG_DATE_FORMAT
        {
            get { return LOG_DATE == DateTime.MinValue? "": ((DateTime?)LOG_DATE).ToTwDateString("yyy/MM/dd HH:mm"); }
        }
        /// <summary>
        /// 主管機關名稱
        /// </summary>
        public string MASTER_ORGAN_NAME { get; set; }
        /// <summary>
        /// 執行機關名稱
        /// </summary>
        public string EXEC_ORGAN_NAME { get; set; }


    }
}
