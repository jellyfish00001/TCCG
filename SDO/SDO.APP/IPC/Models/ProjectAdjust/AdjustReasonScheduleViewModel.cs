using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.IPC.Models.ProjectAdjust
{
    public class AdjustReasonScheduleViewModel : AdjustReasonModel
    {
        /// <summary>
        /// 佐證資料
        /// </summary>
        public new List<ProjectAttachmentModel> Files2 { set; get; }
        /// <summary>
        /// 佐證資料
        /// </summary>
        public new List<ProjectAttachmentModel> Files { set; get; }
        /// <summary>
        /// 管考意見
        /// </summary>
        public string REVIEW_COMMENTS { set; get; }
        /// <summary>
        /// 審查結果
        /// </summary>
        public string REVIEW_RESULT { set; get; }
        /// <summary>
        /// 特殊加註
        /// </summary>
        public string SPEC_NOTE { set; get; }
    }
}
