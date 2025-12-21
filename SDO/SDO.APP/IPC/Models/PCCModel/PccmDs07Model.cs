using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 工程標案工程概要資料 (工程會API欄位)
    /// </summary>
    public class PccmDs07Model
    {
        /// <summary>
        /// 標案代碼
        /// </summary>
        public string plnprj_id { get; set; }

        /// <summary>
        /// 發包預算(千元)
        /// </summary>
        public decimal? bdgt1 { get; set; }
    }
}
