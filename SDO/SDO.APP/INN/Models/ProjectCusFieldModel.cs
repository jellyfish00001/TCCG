using SDO.Base.Utils.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class ProjectCusFieldModel : DbEditor
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public int CUS_FIELD_ID { get; set; }

        /// <summary>
        /// 創新提案年度
        /// </summary>
        public string YEAR { get; set; }

        /// <summary>
        /// 是否使用
        /// </summary>
        public bool IS_USE { get; set; }

        /// <summary>
        /// 項次
        /// </summary>
        public int CUS_ITEM { get; set; }

        /// <summary>
        /// 欄位名稱
        /// </summary>
        public string CUS_FIELD_NANE { get; set; }

    }
}
