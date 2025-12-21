using SDO.APP.IPC.Models.ProjectAdjust;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 管考備註
    /// </summary>
    public class ProjectFillAuditModel
    {
        /// <summary>
        /// 計畫基本資料
        /// </summary>
        public ProjectBasicForProjectFillAuditModel ProjectBasic { get; set; }

        /// <summary>
        /// 管考審核意見
        /// </summary>
        public List<ProjectEngineeringAuditOpinionModel> ProjectEngineeringAuditOpinion { get; set; }

        /// <summary>
        /// 會議列管
        /// </summary>
        public List<ProjectConferenceModel> ProjectConference { get; set; }

        /// <summary>
        /// 逾期繳交填報紀錄
        /// </summary>
        public List<ProjectDelayfillModel> ProjectDelayfill { get; set; }

        /// <summary>
        /// 計畫分併案記錄檔
        /// </summary>
        public List<ProjectMergeLogModel> ProjectMergeLog { get; set; }

        /// <summary>
        /// 撤銷資料
        /// </summary>
        public AdjustAuditModel RevokeData { get; set; }

        /// <summary>
        /// 特殊加註計畫參數值對應資料
        /// </summary>
        public List<ProjectMappingDataModel> SpecNoteMappingData { get; set; }

        /// <summary>
        /// 實地查證情形
        /// </summary>
        public List<ProjectFactFindingModel> ProjectFactFinding { get; set; }

        /// <summary>
        /// 計畫結案明細資料
        /// </summary>
        public List<ProjectCloseDetailsModel> ProjectCloseDetails { get; set; }

        /// <summary>
        /// 結案意見
        /// </summary>
        public ProjectCloseMemoModel ProjectCloseMemo { get; set; }

        /// <summary>
        /// 未於期限內提出計畫調整資料
        /// </summary>
        public List<ProjectBasicAdjForDelayApply> DelayApply { get; set; }
    }
}
