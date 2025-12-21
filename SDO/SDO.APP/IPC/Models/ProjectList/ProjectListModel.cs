using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 資料登錄Model
    /// </summary>
    public class ProjectListModel
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 作業階段(計畫狀態)代碼
        /// </summary>
        public string PROJECT_STATUS_C { get; set; }
        /// <summary>
        /// 作業階段(計畫狀態)
        /// </summary>
        public string PROJECT_STATUS { get; set; }
        /// <summary>
        /// 計畫調整狀態代碼
        /// </summary>
        public string PROJECT_AW_STATUS_C { get; set; }
        /// <summary>
        /// 計畫調整狀態
        /// </summary>
        public string PROJECT_AW_STATUS { get; set; }
        /// <summary>
        /// 計畫年度
        /// </summary>
        public int PROJECT_YEAR { get; set; }
        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PROJECT_NAME { get; set; }
        /// <summary>
        /// 執行機關承辦人
        /// </summary>
        public string EXEC_UNDERTAKER { get; set; }
        /// <summary>
        /// 主管機關代碼
        /// </summary>
        public string MASTER_ORGAN_C { get; set; }
        /// <summary>
        /// 主管機關名稱
        /// </summary>
        public string MASTER_ORGAN_NAME { get; set; }
        /// <summary>
        /// 執行機關排序
        /// </summary>
        public string EXEC_ORGAN_ORDER { get; set; }
        /// <summary>
        /// 執行機關代碼
        /// </summary>
        public string EXEC_ORGAN_C { get; set; }
        /// <summary>
        /// 執行機關名稱
        /// </summary>
        public string EXEC_ORGAN_NAME { get; set; }
        /// <summary>
        /// 計畫總經費
        /// </summary>
        public long BUDGET_TOTAL { get; set; }
        /// <summary>
        /// 釘選
        /// </summary>
        public bool PIS_SELECT { get; set; }
        /// <summary>
        /// 調整流水號
        /// </summary>
        public int PROJ_ADJ_ID { get; set; }
        /// <summary>
        /// 調整申請項目 (SET_PARAM.SET_ITEM = 'AW_KIND')
        /// </summary>
        public string AW_KIND { set; get; }
        /// <summary>
        /// 執行方式
        /// </summary>
        public string CP_KIND { get; set; }
    }
}
