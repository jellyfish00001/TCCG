using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDO.Models;

namespace SDO.Models
{
    /// <summary>
    /// 計畫對應執行機關聯繫資訊資料 Model
    /// </summary>
    public class ProjectOrgContactModel
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 執行機關代碼
        /// </summary>
        public string EXEC_ORGAN_C { get; set; }
        /// <summary>
        /// 聯繫人
        /// </summary>
        public string Contact { get; set; }
        /// <summary>
        /// 信箱
        /// </summary>
        public string Email { get; set; }
    }
}
