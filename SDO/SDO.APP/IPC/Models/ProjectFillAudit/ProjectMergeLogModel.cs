using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫分併案記錄檔
    /// </summary>
    public class ProjectMergeLogModel : DbEditor
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
        /// 分案或併案
        /// </summary>
        public string MERGE_STATUS { get; set; }

        /// <summary>
        /// 分案併案日期
        /// </summary>
        public DateTime? PROMERGE_DATE { get; set; }

        /// <summary>
        /// 分案併案附件
        /// </summary>
        public List<ProjectAttachmentModel> File { get; set; }
    }
}
