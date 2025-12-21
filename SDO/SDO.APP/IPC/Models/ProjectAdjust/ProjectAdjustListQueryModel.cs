using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.IPC.Models.ProjectAdjust
{
    public class ProjectAdjustListQueryModel
    {
        /// <summary>
        /// 是否為審核頁面 0: 填報頁面，1: 審核頁面
        /// </summary>
        public int IsReview { get; set; }
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 作業階段(計畫狀態)
        /// </summary>
        public List<string> PROJECT_AW_STATUS { get; set; }
        /// <summary>
        /// 計畫年度
        /// </summary>
        public int PROJECT_YEAR { get; set; }
        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PROJECT_NAME { get; set; }
        /// <summary>
        /// 執行機關
        /// </summary>
        public string EXEC_ORGAN_C { get; set; }
        /// <summary>
        /// 執行方式類別
        /// </summary>
        public string CP_KIND { get; set; }
        /// <summary>
        /// 執行方式
        /// </summary>
        public string RUNWAY_C { get; set; }
        /// <summary>
        /// 特殊加註
        /// </summary>
        public string SPEC_NOTE { get; set; }
    }
}
