using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class AuditTemplateModel : DbEditor
    {
        /// <summary>
        /// 計畫性值
        /// </summary>
        public string PLANDATETYPE { get; set; }

        /// <summary>
        /// 計畫性值預算執行類別
        /// </summary>
        public string SUB_PLANDATETYPE { get; set; }

        /// <summary>
        /// 關聯性
        /// </summary>
        public string PLAN_REF { get; set; }

        /// <summary>
        /// 執行績效
        /// </summary>
        public string EXE_PERFORMANCE { get; set; }

        /// <summary>
        /// 範例
        /// </summary>
        public string TEMPLATE { get; set; }

    }
}

