using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫基本資料異動記錄檔
    /// </summary>
    public class ProjectBasicLogModel : DbEditor
    {
        /// <summary>
        /// 列管編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PROJECT_NAME { get; set; }

        /// <summary>
        /// 計畫年度
        /// </summary>
        public string PROJECT_YEAR { get; set; }

        /// <summary>
        /// 計畫狀態
        /// </summary>
        public string PROJECT_STAGE { get; set; }

        /// <summary>
        /// 異動狀態名稱
        /// </summary>
        public string LOG_STATUS { get; set; }

        /// <summary>
        /// 異動狀態代碼
        /// </summary>
        public string LOG_STATUS_C { get; set; }

        public string MEMO { get; set; }
        /// <summary>
        /// 異動當下人員一級機關
        /// </summary>
        public string MDF_ORG_NAME { get; set; }

    }
}
