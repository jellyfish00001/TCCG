using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDO.Models;

namespace SDO.Models
{
    /// <summary>
    /// 重大工程進度落後狀況
    /// </summary>
    public class MajorProjectDelayStatusModel : DbEditor
    {
        /// <summary>
        /// 專案編號
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 專案名稱
        /// </summary>
        public string PROJECT_NAME { get; set; }
        /// <summary>
        /// 執行機關
        /// </summary>
        public string EXEC_DEPT { get; set; }
        /// <summary>
        /// 計畫總經費
        /// </summary>
        public decimal BUDGET_TOTAL { get; set; }
        /// <summary>
        /// 中央補助
        /// </summary>
        public decimal CENTRAL_SUBSIDY { get; set; }
        /// <summary>
        /// 本府預算
        /// </summary>
        public decimal LOCAL_BUDGET { get; set; }
        /// <summary>
        /// 檢核點(預定/實際)
        /// </summary>
        public string SCHEDULE_ACTUAL { get; set; }
        /// <summary>
        /// 進度落後天數
        /// </summary>
        public int CHKPT_DELAY_DAYS { get; set; }
        /// <summary>
        /// 工程進度(預定/實際/差異)
        /// </summary>
        public string ENGINEERING_PROGRESS_IPC_RAD_PRG { get; set; }
        /// <summary>
        /// 落後類型
        /// </summary>
        public string DELAY_TYPE { get; set; }
        /// <summary>
        /// 落後原因
        /// </summary>
        public string DELAY_CAUSAL { get; set; }
        /// <summary>
        /// 解決方案
        /// </summary>
        public string SOLUTION { get; set; }
        /// <summary>
        /// 須協調事項
        /// </summary>
        public string COORDINATION { get; set; }
    }
}
