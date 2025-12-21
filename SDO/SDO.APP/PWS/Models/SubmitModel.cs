using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class SubmitModel
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public int PROJECT_NO { get; set; }

        /// <summary>
        /// 是否送出
        /// </summary>
        public bool IS_SEND { get; set; }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public List<ProjectSubmitErrorModel> ErrorModels { get; set; }
    }
}

