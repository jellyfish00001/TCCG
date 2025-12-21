using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 立案審核 Model
    /// </summary>
    public class ProjectFillAddAuditModel: DbEditor
    {
        /// <summary>
        /// 列管編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 計畫狀態
        /// </summary>
        public string REVIEW_RESULT { get; set; }

        /// <summary>
        /// 管考意見
        /// </summary>
        public string MEMO_EVALUATION { get; set; }

        /// <summary>
        /// 特殊加註資料
        /// </summary>
        public List<ProjectMappingDataModel> SpecNoteDatas { get; set; }
        /// <summary>
        /// 存檔類別 1: 存檔 2: 送出
        /// </summary>
        public int SaveType { get; set; }

        /// <summary>
        /// 計畫歷程清單
        /// </summary>
        public List<ProjectLogListModel> ProjLogs { get; set; }
    }
}
