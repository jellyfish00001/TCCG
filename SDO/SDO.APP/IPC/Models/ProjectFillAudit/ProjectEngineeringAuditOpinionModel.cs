using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 管考審核意見
    /// </summary>
    public class ProjectEngineeringAuditOpinionModel : DbEditor
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public int SEQ { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 年份
        /// </summary>
        public string YEAR { get; set; }

        /// <summary>
        /// 月份
        /// </summary>
        public string MONTH { get; set; }

        /// <summary>
        /// 管考意見
        /// </summary>
        public string AUDIT_OPINION { get; set; }

        /// <summary>
        /// 核定結果
        /// </summary>
        public string CHECK_RESULT { get; set; }

        /// <summary>
        /// 其它備註
        /// </summary>
        public string IMPROVEMENT_OPINION { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        public List<ProjectMappingDataModel> ComIPCMemoMappingData { get; set; }
    }
}
