using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 預覽列印報表查詢
    /// </summary>
    public class InnProjectPrintQueryModel
    {
        public string Year { get; set; }
        /// <summary>
        /// 計畫編號
        /// </summary>
        public List<string> INN_PLAN_NO { get; set; }

        public string FILE_NAME { get; set; }
    }
}
