using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.IPC.Models.ProjectAdjust
{
    /// <summary>
    /// 主辦申請調整基本資料原因Model
    /// </summary>
    public class AdjustReasonViewModel
    {
        /// <summary>
        /// 計畫調整檔流水號
        /// </summary>
        public int PROJ_ADJ_ID { set; get; }
        /// <summary>
        /// 列管編號
        /// </summary>
        public string PROJECT_NO { set; get; }
        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PROJECT_NAME { set; get; }
        /// <summary>
        /// 調整申請項目 (SET_PARAM.SET_ITEM = 'AW_KIND')
        /// </summary>
        public string AW_KIND { set; get; }
        /// <summary>
        /// 核准日期
        /// </summary>
        public DateTime? APPRV_DATE { set; get; }
        /// <summary>
        /// 計畫調整原因說明
        /// </summary>
        public string ADJUST_REASON { set; get; }
        /// <summary>
        /// 其他調整原因
        /// </summary>
        public string OTHER_REASON { set; get; }
        /// <summary>
        /// 調整撤銷狀態
        /// </summary>
        public string PROJECT_AW_STATUS { set; get; }
        /// <summary>
        /// 管考意見
        /// </summary>
        public string REVIEW_COMMENTS { set; get; }
        /// <summary>
        /// 審查結果
        /// </summary>
        public string REVIEW_RESULT { set; get; }
        /// <summary>
        /// 調整原因/撤銷原因 (用於放入PROJECT_MAPPING_DATA)
        /// </summary>
        public List<ProjectMappingDataModel> Reasons { set; get; }
        /// <summary>
        /// 計畫檔案資料
        /// </summary>
        public List<ProjectAttachmentModel> Files { set; get; }
    }
}
