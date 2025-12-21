using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫結案明細資料
    /// </summary>
    public class ProjectCloseDetailsModel : DbEditor
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public int SEQ { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 結案管考項目類別
        /// </summary>
        public string CLOSE_DETAILS_TYPE { get; set; }

        /// <summary>
        /// 參考日期
        /// </summary>
        public DateTime? REF_DATE { get; set; }

        /// <summary>
        /// 參考說明
        /// </summary>
        public string REF_MEMO { get; set; }
    }
}
