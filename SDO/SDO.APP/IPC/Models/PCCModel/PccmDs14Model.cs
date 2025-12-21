using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 工程標案驗收資料 (工程會API欄位)
    /// </summary>
    public class PccmDs14Model
    {
        /// <summary>
        /// 標案代碼
        /// </summary>
        public string plnprj_id { get; set; }

        /// <summary>
        /// 辦理驗收 預定日期
        /// </summary>
        public DateTime? idate { get; set; }

        /// <summary>
        /// 驗收開始日期
        /// </summary>
        public DateTime? aokdat { get; set; }

        /// <summary>
        /// 資料更新時間
        /// </summary>
        public DateTime? updatetime { get; set; }
    }
}
