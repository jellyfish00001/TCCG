using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 執行情形送出檢核Model
    /// </summary>
    public class ProjectExecuteSubmitModel : ProjectSubmitResultModel
    {
        /// <summary>
        /// 是否可提出結案申請
        /// </summary>
       public bool projectCloseApplyValid { get; set; }
    }
}
