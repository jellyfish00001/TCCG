using SDO.APP.IPC.Models.ProjectAdjust;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 其他非相關檔案上傳的檔案資訊
    /// </summary>
    public class ProjectFillFileUpModel
    {
        /// <summary>
        /// 非相關檔案上傳的檔案資訊
        /// </summary>
        public List<ProjectOtherAttachmentModel> ProjectOtherAttachmentModels { get; set; }

        /// <summary>
        /// 總期程/分月期程調整歷程
        /// </summary>
        public List<AdjustScheHistoryModel> AdjustScheHistoryModels { get; set; }

    }
}
