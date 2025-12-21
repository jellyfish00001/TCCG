using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class PWSReportQueryModel : DbEditor
    {
        public string Year { get; set; }
        /// <summary>
        /// 計畫編號
        /// </summary>
        public List<string> INN_PLAN_NO { get; set; }

    }
}
