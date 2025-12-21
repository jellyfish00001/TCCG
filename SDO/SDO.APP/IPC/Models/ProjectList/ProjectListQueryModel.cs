using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 資料登錄查詢Model
    /// </summary>
    public class ProjectListQueryModel:DbEditor
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 作業階段(計畫狀態)
        /// </summary>
        public List<string> PROJECT_STATUS{ get; set; }
        /// <summary>
        /// 計畫年度
        /// </summary>
        public int PROJECT_YEAR { get; set; }
        /// <summary>
        /// 計畫年度狀態 Null:無 A:含之前所有案件、B:含之前未結案件
        /// </summary>
        public string PROJECT_YEAR_STATUS { get; set; }
        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PROJECT_NAME { get; set; }
        /// <summary>
        /// 主管機關
        /// </summary>
        public string MASTER_DEPT { get; set; }
        /// <summary>
        /// 執行機關
        /// </summary>
        public string EXEC_DEPT { get; set; }
        /// <summary>
        /// 協辦機關
        /// </summary>
        public string ASS_DEPT { get; set; }
        /// <summary>
        /// 代辦機關
        /// </summary>
        public string AGCY_DEPT { get; set; }
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
        /// <summary>
        /// 是否為調整用清單(0: 否，1: 是)
        /// </summary>
        public int isAdjustList { get; set; }

    }
}
