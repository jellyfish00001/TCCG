using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 工程會異動檢核點設定Model
    /// </summary>
    public class PCCProjChkItemModel:DbEditor
    {
        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 控制點
        /// </summary>
        public string CTRL_POINT { get; set; }

        /// <summary>
        /// 工程會預定完成日期
        /// </summary>
        public DateTime? PCC_ESTIMATED_ENDDATE { get; set; }

        /// <summary>
        /// 工程會實際完成日期
        /// </summary>
        public DateTime? PCC_ACTUAL_ENDDATE { get; set; }

        /// <summary>
        /// 實際完成日期
        /// </summary>
        public DateTime? ACTUAL_ENDDATE { get; set; }

        public string PROJECT_NAME { get; set; }
        public string PROJECT_YEAR { get; set; }
        public DateTime? MDF_DATE { get; set; }
    }
}
