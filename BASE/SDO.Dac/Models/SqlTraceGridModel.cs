using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 查詢SQL日誌結果MODEL
    /// </summary>
    public class SqlTraceGridModel
    {
        /// <summary>
        /// SQL日誌總數量統計
        /// </summary>
        public int DATA_COUNT { get; set; }

        /// <summary>
        /// SQL日誌作者ID
        /// </summary>
        public string USER_ID { get; set; }

        /// <summary>
        /// SQL日誌作者名稱
        /// </summary>
        public string USER_NAME { get; set; }

        /// <summary>
        /// SQL日誌作者IP
        /// </summary>
        public string USER_IP { get; set; }

        /// <summary>
        /// SQL指令內容
        /// </summary>
        public string COMMANDTEXT { get; set; }

        /// <summary>
        /// SQL指令參數
        /// </summary>
        public string PARAMETERS { get; set; }

        /// <summary>
        /// 發起要求的路徑
        /// </summary>
        public string REQUEST_URL { get; set; }

        /// <summary>
        /// SQL日誌紀錄日期
        /// </summary>
        public string LOG_DATE { get; set; }
    }
}
