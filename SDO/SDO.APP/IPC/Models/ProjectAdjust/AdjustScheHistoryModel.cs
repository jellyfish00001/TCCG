using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.IPC.Models.ProjectAdjust
{
	/// <summary>
	/// 期程調整歷程model
	/// </summary>
    public class AdjustScheHistoryModel
    {
        /// <summary>
        /// 第幾次總期程/分月期程調整
        /// </summary>
        public int SEQ { get; set; }
        /// <summary>
        /// 調整流水號
        /// </summary>
        public int PROJ_ADJ_ID { get; set; }
        /// <summary>
        /// 列管編號
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 調整類別(Y: 總期程調整、M: 分月期程調整)
        /// </summary>
        public string SCHE_TYPE { get; set; }
        /// <summary>
        /// 調整撤銷狀態
        /// </summary>
        public string PROJECT_AW_STATUS { set; get; }
        /// <summary>
        /// 核准日期
        /// </summary>
        public DateTime? APPRV_DATE { set; get; }
        /// <summary>
        /// 調整原因
        /// </summary>
        public string REASON { set; get; }
        /// <summary>
        /// 原最後一個預定完成日期(僅取總期程的原預定完成日)
        /// </summary>
        public DateTime? ORI_ESTIMATED_ENDDATE { get; set; }
        /// <summary>
        /// 最後一個調整後預定完成日期
        /// 總期程: PROJGRESS=100 的 ESTIMATED_ENDDATE
        /// 分月調整: 所有有調整過的檢核點中最後一個ESTIMATED_ENDDATE
        /// </summary>
        public DateTime? ADJ_LAST_DATE { get; set; }
        /// <summary>
        /// 管考審核結果(Y: 審核通過、N: 審核不通過、R: 退回補正)
        /// </summary>
        public string REVIEW_RESULT { set; get; }
        /// <summary>
        /// 管考意見
        /// </summary>
        public string REVIEW_COMMENTS { set; get; }
    }
}
