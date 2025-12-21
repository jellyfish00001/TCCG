using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class FundingExecutionModel : DbEditor
    {
        /// <summary>
        /// 計畫序號
        /// </summary>
        public string PLANNO { get; set; }


        /// <summary>
        /// 計畫總經費
        /// </summary>
        public int? TOTAL { get; set; }


        /// <summary>
        /// 經費需求細項model
        /// </summary>
        public List<DAMTBModel> DAMTBListModel { get; set; }

        /// <summary>
        /// 歷年執行情形model
        /// </summary>
        public List<BudgetExecModel> budgetExecListModel { get; set; }

    }
}
