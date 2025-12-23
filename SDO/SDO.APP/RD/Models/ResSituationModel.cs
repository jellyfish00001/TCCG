using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 取得參採情形/結案成果填報/續列管一年參採情形 Model
    /// </summary>
    public class ResSituationModel : DbEditor
    {
        /// <summary>
        /// 送審資料
        /// </summary>
        public RDAuditStatusModel AUDIT { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PLAN_NO { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PLAN_NAME { get; set; }

        /// <summary>
        /// 參採情形 / 結案初步評估採行情形 1.採行、2.參採、3.存查
        /// </summary>
        public string SITUATION_TYPE { get; set; }

        /// <summary>
        /// 採行情形簡述
        /// </summary>
        public string SITUATION_DESC { get; set; }

        /// <summary>
        /// 續列管一年參採情形
        /// </summary>
        public string CONTINUE_SITUAITON_DESC { get; set; }

        /// <summary>
        /// 續列管一年內採行情形
        /// </summary>
        public string CONTINUE_SITUAITON_TYPE { get; set; }

        /// <summary>
        /// 結案成果填報 審核狀態
        /// </summary>
        public string SITUATION_STATUS { get; set; }

        /// <summary>
        /// 續列管一年內採行情形 審核狀態
        /// </summary>
        public string SITUACONTINUE_STATUS { get; set; }

        /// <summary>
        /// 評核指標數量
        /// </summary>
        /// <remarks>>透過此值判斷結案成果填報結果是否可審</remarks>
        public int RD_RES_POLICY_INDEX_COUNT { get; set; }

        /// <summary>
        /// 評核指標審核通過數量
        /// </summary>
        /// <remarks>>透過此值判斷結案成果填報結果是否可審</remarks>
        public int STATUS_3_COUNT { get; set; }

        /// <summary>
        /// 結案日期
        /// </summary>
        public DateTime? CLOSING_DATE { get; set; }

        /// <summary>
        /// 資料鎖定
        /// </summary>
        public string LOCK_YN { get; set; }

    }
}
