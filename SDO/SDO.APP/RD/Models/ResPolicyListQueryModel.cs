using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 取得執行情形清單 Model
    /// </summary>
    public class ResPolicyListQueryModel : DbEditor
    {
        /// <summary>
        /// 計畫流水號
        /// </summary>
        public int SEQ { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PLAN_NO { get; set; }

        /// <summary>
        /// 年度
        /// </summary>
        public string PLAN_YEAR { get; set; }

        /// <summary>
        /// 季別
        /// </summary>
        public string SEASON_TYPE { get; set; }

        /// <summary>
        /// 預期完成期程
        /// </summary>
        public string RES_FINISH_DATE { get; set; }

        /// <summary>
        /// 評核類別
        /// </summary>
        public string POLICY_KIND { get; set; }

        /// <summary>
        /// 評核指標
        /// </summary>
        public string POLICY_INDEX_DESC { get; set; }

        /// <summary>
        /// 評核指標審核狀態 - 轉參數名稱
        /// </summary>
        public string STATUS { get; set; }

        /// <summary>
        /// 評核指標審核狀態 - 參數代碼
        /// </summary>
        public string POLICY_STATUS { get; set; }

        /// <summary>
        /// 基本資料審核狀態 - 參數代碼
        /// </summary>
        /// <remarks>透過此值判斷評核指標是否可審</remarks>
        public string RESEARCH_STATUS { get; set; }
    }
}
