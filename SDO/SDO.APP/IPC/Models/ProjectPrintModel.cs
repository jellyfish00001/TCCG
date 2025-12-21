using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 預覽列印
    /// </summary>
    public class ProjectPrintModel
    {
        /// <summary>
        /// 基本資料
        /// </summary>
        public ProjectBasicFillModel ProjectBasicFill { get; set; }

        /// <summary>
        /// 檢核點設定(基本資料)
        /// </summary>
        public ProjectCheckpointModel ProjectCheckpoint { get; set; }

        /// <summary>
        /// 檢核點(執行情形)
        /// </summary>
        public ProjectFillCkptComModel ProjectCkptCom { get; set; }

        /// <summary>
        /// 每月辦理情形(單筆)
        /// </summary>
        public ProjectEngineeringProgressTableModel CheckProjectEngineeringProgress { get; set; }

        /// <summary>
        /// 每月辦理情形
        /// </summary>
        public List<ProjectEngineeringProgressGridModel> ProjectEngineeringProgress { get; set; }

        /// <summary>
        /// 落後原因分析
        /// </summary>
        public List<ProjectDelayCausalModel> ProjectDelay { get; set; }

        /// <summary>
        /// 預算執行情形
        /// </summary>
        public List<ProjectBudgetExecuteModel> ProjectBudgetExecute { get; set; }

        /// <summary>
        /// 實地查證情形
        /// </summary>
        public List<ProjectFactFindingModel> ProjectFactFinding { get; set; }

        /// <summary>
        /// 其它資訊
        /// </summary>
        public ProjectFillOtherModel ProjectOther { get; set; }

        /// <summary>
        /// 結案資料
        /// </summary>
        public ProjectFillCloseModel ProjectClose { get; set; }

        /// <summary>
        /// 管考備註
        /// </summary>
        public ProjectFillAuditModel ProjectAudit { get; set; }

        /// <summary>
        /// 顯示清單
        /// </summary>

        public Dictionary<string, object> ShowList { get; set; }
    }
}
