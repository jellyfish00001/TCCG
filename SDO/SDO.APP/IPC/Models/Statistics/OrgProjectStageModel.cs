using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 機關列管計畫資料
    /// </summary>
    public class OrgProjectStageModel
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 是否使用界接填報
        /// </summary>
        public bool IS_USER_FTY_DATA { get; set; }
        /// <summary>
        /// 機關代碼
        /// </summary>
        public string EXEC_ORGAN_C { get; set; }
        /// <summary>
        /// 機關名稱
        /// </summary>
        public string EXEC_ORGAN_NAME { get; set; }
        /// <summary>
        /// 計畫階段
        /// </summary>
        public string PROJECT_STAGE { get; set; }
    }
}
