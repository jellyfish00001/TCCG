using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDO.Models;

namespace SDO.Models
{
    public class MajorProjectManageStatusModel : DbEditor
    {
        /// <summary>
        /// 專案編號
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 專案名稱
        /// </summary>
        public string PROJECT_NAME { get; set; }
        /// <summary>
        /// 列管狀態
        /// </summary>
        public string STAGE { get; set; }
        /// <summary>
        /// 機關
        /// </summary>
        public string EXEC_DEPT { get; set; }
        /// <summary>
        /// 計畫總經費
        /// </summary>
        public decimal BUDGET_TOTAL { get; set; }
        /// <summary>
        /// 中央補助
        /// </summary>
        public decimal CENTRAL_SUBSIDY { get; set; }
        /// <summary>
        /// 本府預算
        /// </summary>
        public decimal LOCAL_BUDGET { get; set; }
        /// <summary>
        /// 地區別
        /// </summary>
        public string TOWNNAME { get; set; }
        /// <summary>
        /// 主要建設
        /// </summary>
        public string MAIN_CONSTRUCTION { get; set; }
        /// <summary>
        /// 附屬設施
        /// </summary>
        public string SUBSIDIARY_FACILITY { get; set; }
        /// <summary>
        /// 落後類型
        /// </summary>
        public string DELAY_TYPE { get; set; }
        /// <summary>
        /// 結案/撤銷日期
        /// </summary>
        public string CLOSURE_CANCELLATION_DATE { get; set; }

    }
}
