using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 表5 特定檢核點屆期情形
    /// </summary>
    public class ProjectUnFilledModel
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
        /// 執行機關名稱
        /// </summary>
        public string EXEC_ORGAN_NAME { get; set; }

        /// <summary>
        /// 承辦人
        /// </summary>
        public string REAL_CONTACT { get; set; }

        /// <summary>
        /// 電話
        /// </summary>
        public string REAL_TEL { get; set; }

        /// <summary>
        /// 使用國發會界接資料 有使用:true 沒使用:false
        /// </summary>
        public bool IS_USER_FTY_DATA { get; set; }
    }
}
