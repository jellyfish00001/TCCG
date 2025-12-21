using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫送出Model
    /// </summary>
    public class ProjectSubmitResultModel
    {
        /// <summary>
        /// 是否已送出
        /// </summary>
        public bool IsSubmitted { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public List<ProjectSubmitErrorModel> ErrorModels { get; set; }
    }
}
