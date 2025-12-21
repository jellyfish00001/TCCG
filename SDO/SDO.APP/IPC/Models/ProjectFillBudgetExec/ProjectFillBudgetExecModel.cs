using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 預算執行情形
    /// </summary>
    public class ProjectFillBudgetExecModel
    {
        /// <summary>
        /// 填報週期
        /// </summary>
        public ProjectFillCycleModel ProjectFillCycle { get; set; }

        /// <summary>
        /// 預算執行情形累計執行情形
        /// </summary>
        public List<ProjectBudgetExecuteModel> ProjectBudgetExecute { get; set; }
    }
}
