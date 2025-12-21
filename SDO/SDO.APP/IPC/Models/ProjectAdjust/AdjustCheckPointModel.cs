using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.IPC.Models.ProjectAdjust
{
    public class AdjustCheckPointModel : ProjectCheckpointModel
    {
        /// <summary>
        /// 調整檔流水號
        /// </summary>
        public int PROJ_ADJ_ID { get; set; }
        /// <summary>
        /// 調整類別(Y: 總期程調整、M: 分月期程調整)
        /// </summary>
        public string SCHE_TYPE { get; set; }
        /// <summary>
        /// 自訂檢核點資料
        /// </summary>
        public new List<AdjustCusCheckPointModel> CusCheckpointModels { get; set; }
        /// <summary>
        /// 工程竣工報告表或函報竣工文件(FILE_KIND = 13)
        /// </summary>
        public List<ProjectAttachmentModel> Files { set; get; }
    }
}
