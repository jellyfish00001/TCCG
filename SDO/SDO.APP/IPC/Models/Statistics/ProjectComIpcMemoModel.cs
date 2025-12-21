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
    public class ProjectComIpcMemoModel
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
        /// 統計年
        /// </summary>
        public string YEAR { get; set; }
        /// <summary>
        /// 統計月
        /// </summary>
        public string MONTH { get; set; }
        /// <summary>
        /// 平時管考意見備註代碼
        /// </summary>
        public string COM_IPCMEMO { get; set; }
        /// <summary>
        /// 執行機關代碼
        /// </summary>
        public string EXEC_ORGAN_C { get; set; }
        /// <summary>
        /// 執行機關名稱
        /// </summary>
        public string EXEC_ORGAN_NAME { get; set; }
    }
}
