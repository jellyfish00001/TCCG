using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計劃自訂查核點Model
    /// </summary>
    public class ProjectCusCheckpointModel:DbEditor
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public int SEQ { get; set; }
        /// <summary>
        /// 計劃編號
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 有值則對應CODE_CHECKPOINT_ITEM.SEQ，否則使用者新增
        /// </summary>
        public int CHECKITEM_SEQ { get; set; }
        /// <summary>
        /// 檢核點
        /// </summary>
        public string CHECKITEM_NAME { get; set; }
        /// <summary>
        /// 管考進度
        /// </summary>
        public decimal PROGRESS { get; set; }
        /// <summary>
        /// 預計開始日期
        /// </summary>
        public DateTime? ESTIMATED_STARTDATE { get; set; }
        /// <summary>
        /// 預定完成日期
        /// </summary>
        public DateTime? ESTIMATED_ENDDATE { get; set; }
        /// <summary>
        /// 工程會預計完成日期
        /// </summary>
        public DateTime? PCC_ESTIMATED_ENDDATE { get; set; }
        /// <summary>
        /// 工程會實際完成日期
        /// </summary>
        public DateTime? PCC_ACTUAL_ENDDATE { get; set; }
        /// <summary>
        /// 控制點
        /// </summary>
        public string CTRL_POINT { get; set; }
        /// <summary>
        /// 實際完成日期
        /// </summary>
        public DateTime? ACTUAL_ENDDATE { get; set; }
        /// <summary>
        /// 實際完成日期(舊)
        /// </summary>
        public DateTime? ACTUAL_ENDDATE_OLD { get; set; }
        /// <summary>
        /// 實際完成日期(存檔用)
        /// </summary>
        public DateTime? ACTUAL_ENDDATE_FOR_SAVE { get; set; }
        /// <summary>
        /// 是否落後
        /// </summary>
        public int IS_DELAY { get; set; }
        /// <summary>
        /// 資料建立日期
        /// </summary>
        public DateTime CRT_DATE { get; set; }
        /// <summary>
        /// 填報日期
        /// </summary>
        public DateTime MDF_DATE { get; set; }
    }
}
