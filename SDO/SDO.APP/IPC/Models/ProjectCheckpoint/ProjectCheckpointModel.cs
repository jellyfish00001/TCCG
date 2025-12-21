using SDO.APP.IPC.Models.ProjectAdjust;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class ProjectCheckpointModel:DbEditor
    {
        /// <summary>
        /// 計劃編號
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PROJECT_NAME { get; set; }
        /// <summary>
        /// 執行方式類別
        /// </summary>
        public string CP_KIND { get; set; }
        /// <summary>
        /// 執行方式
        /// </summary>
        public string RUNWAY_C { get; set; }
        /// <summary>
        /// 執行方式(舊)
        /// </summary>
        public string OLD_RUNWAY_C { get; set; }
        /// <summary>
        /// 預定完成期限
        /// </summary>
        public DateTime? PROJECT_LAST_DATE { get; set; }
        /// <summary>
        /// 備註
        /// </summary>
        public string MEMO_CHK_POINT { get; set; }
        /// <summary>
        /// 計劃開始日期
        /// </summary>
        public DateTime? CONTROL_DATE1 { get; set; }
        /// <summary>
        /// 最後一項檢核點的預計完成日期
        /// </summary>
        public DateTime? CONTROL_DATE6 { get; set; }
        /// <summary>
        /// 總期程/分月期程調整歷程
        /// </summary>
        public List<AdjustScheHistoryModel> AdjustScheHistoryModels { get; set; }
        /// <summary>
        /// 自訂檢核點資料
        /// </summary>
        public List<ProjectCusCheckpointModel> CusCheckpointModels { get; set; }
    }
}
