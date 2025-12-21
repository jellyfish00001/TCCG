using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫基本資料
    /// </summary>
    public class ProjectBasicModel : DbEditor
    {
        /// <summary>
        /// 列管編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PROJECT_NAME { get; set; }
        /// <summary>
        /// 計畫年度
        /// </summary>
        public string PROJECT_YEAR { get; set; }

        /// <summary>
        /// 計畫屬性，預設'0'
        /// </summary>
        public int PROJECT_TYPE { get; set; }

        /// <summary>
        /// 主管機關
        /// </summary>
        public string MASTER_ORGAN_C { get; set; }

        /// <summary>
        /// 主管機關人員/主管機關承辦人
        /// </summary>
        public string MASTER_UNDERTAKER_C { get; set; }

        /// <summary>
        /// 主管機關人員/主管機關承辦人(名稱)
        /// </summary>
        public string MASTER_UNDERTAKER_NAME { get; set; }

        /// <summary>
        /// 執行機關
        /// </summary>
        public string EXEC_ORGAN_C { get; set; }

        /// <summary>
        /// 執行機關人員/執行機關承辦人
        /// </summary>
        public string EXEC_UNDERTAKER_C { get; set; }

        /// <summary>
        /// 執行機關人員/執行機關承辦人(名稱)
        /// </summary>
        public string EXEC_UNDERTAKER_NAME { get; set; }

        /// <summary>
        /// 代辦機關
        /// </summary>
        public string BUDGET_HOLD_ORGAN_C { get; set; }

        /// <summary>
        /// 代辦機關承辦人
        /// </summary>
        public string BUDGET_HOLD_UNDERTAKER_C { get; set; }

        /// <summary>
        /// 代辦機關承辦人(名稱)
        /// </summary>
        public string BUDGET_HOLD_UNDERTAKER_NAME { get; set; }

        /// <summary>
        /// 相關審查
        /// </summary>
        public string REVIEWITEM { get; set; }

        /// <summary>
        /// 辦理地點
        /// </summary>
        public string TOWN_C { get; set; }
        /// <summary>
        /// 辦理地點(跨區)
        /// </summary>
        public string TOWN_M { get; set; }

        /// <summary>
        /// 位置說明
        /// </summary>
        public string PROJECT_LOCATION { get; set; }

        /// <summary>
        /// 單點地圖定位-坐標X
        /// </summary>
        public string X_COORD { get; set; }

        /// <summary>
        /// 單點地圖定位-坐標Y
        /// </summary>
        public string Y_COORD { get; set; }

        /// <summary>
        /// 計畫內容
        /// </summary>
        public string ALL_JOB { get; set; }

        /// <summary>
        /// 計畫效益
        /// </summary>
        public string PROJECT_BENEFIT { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        public string MEMO { get; set; }

        /// <summary>
        /// 立案時間
        /// </summary>
        public DateTime? CREATEDTIME { get; set; }

        /// <summary>
        /// 計劃狀態
        /// </summary>
        public string PROJECT_STATUS { get; set; }

        /// <summary>
        /// 管考備註
        /// </summary>
        public string MEMO_EVALUATION { get; set; }

        /// <summary>
        /// 特殊加註
        /// </summary>
        public string SPEC_NOTE { get; set; }

        /// <summary>
        /// 立案審核意見
        /// </summary>
        public string REVIEW_COMMENTS { get; set; }

        /// <summary>
        /// 基本資料成績
        /// </summary>
        public string SCORE_A { get; set; }

        public List<ProjectLogListModel> ProjLogs { get; set; }

        /// <summary>
        /// 列管狀態
        /// </summary>
        public string TUBE_STATUS_DESC { get; set; }
    }
}
