using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 新表二、表三 每月案件地區/機關統計表(詳版)
    /// </summary>
    public class ProjectAreaDeptDetailedModel : ProjectAreaDeptShortModel
    {
        /// <summary>
        /// 建設類別
        /// </summary>
        public string CHECKPOINT_CLASS { get; set; }

        /// <summary>
        /// 特殊加註
        /// </summary>
        public string SPEC_NOTE { get; set; }

        /// <summary>
        /// 檢核點名稱
        /// </summary>
        public string CHECKITEM_NAME { get; set; }

        /// <summary>
        /// 預定完成日期
        /// </summary>
        public DateTime? ESTIMATED_ENDDATE { get; set; }

        /// <summary>
        /// 實際完成日期
        /// </summary>
        public DateTime? ACTUAL_ENDDATE { get; set; }

        /// <summary>
        /// 預定施工進度
        /// </summary>
        public Decimal IPC_RES_PRG { get; set; }

        /// <summary>
        /// 實際施工進度
        /// </summary>
        public Decimal IPC_ACT_PRG { get; set; }

        /// <summary>
        /// 執行情形
        /// </summary>
        public string EXECUTE_CONDITION { get; set; }

        /// <summary>
        /// 落後原因
        /// </summary>
        public string DELAY_CAUSAL { get; set; }

        /// <summary>
        /// 檢核點(預定)
        /// </summary>
        public string RES_CHECKITEM { get; set; }

        /// <summary>
        /// 檢核點(實際)
        /// </summary>
        public string ACT_CHECKITEM { get; set; }
    }
}
