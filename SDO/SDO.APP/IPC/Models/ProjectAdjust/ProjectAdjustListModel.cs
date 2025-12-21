using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.IPC.Models.ProjectAdjust
{
    public class ProjectAdjustListModel
    {
        /// <summary>
        /// 調整流水號
        /// </summary>
        public int PROJ_ADJ_ID { get; set; }
        /// <summary>
        /// 調整申請項目 (SET_PARAM.SET_ITEM = 'AW_KIND')
        /// </summary>
        public string AW_KIND { get; set; }
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PROJECT_NAME { get; set; }
        /// <summary>
        /// 計畫年度
        /// </summary>
        public int PROJECT_YEAR { get; set; }
        /// <summary>
        /// 計畫總經費
        /// </summary>
        public long BUDGET { get; set; }
        /// <summary>
        /// 計畫調整狀態代碼
        /// </summary>
        public string PROJECT_AW_STATUS_NAME { get; set; }
        /// <summary>
        /// 計畫調整狀態
        /// </summary>
        public string PROJECT_AW_STATUS { get; set; }
        /// <summary>
        /// 主管機關代號
        /// </summary>
        public string MASTER_ORG_C { get; set; }
        /// <summary>
        /// 執行機關代號
        /// </summary>
        public string EXEC_ORG_C { get; set; }
        /// <summary>
        /// 主管機關名稱
        /// </summary>
        public string MASTER_ORG_NAME { get; set; }
        /// <summary>
        /// 執行機關名稱
        /// </summary>
        public string EXEC_ORG_NAME { get; set; }
        /// <summary>
        /// 執行機關排序
        /// </summary>
        public string EXEC_ORG_ORDER { get; set; }
        /// <summary>
        /// 最後異動時間
        /// </summary>
        public DateTime MDF_DATE { get; set; }
    }
}
