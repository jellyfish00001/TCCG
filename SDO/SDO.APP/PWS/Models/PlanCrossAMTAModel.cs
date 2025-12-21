using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class PlanCrossAMTAModel : DbEditor
    {
        
        /// <summary>
        /// 表單序號
        /// </summary>
        public int IDENTITYFIELD { get; set; }

        /// <summary>
        /// 計畫序號
        /// </summary>
        public string PLANNO { get; set; }

        /// <summary>
        /// 經費年度
        /// </summary>
        public string PLANYEAR { get; set; }

        /// <summary>
        /// 預算來源
        /// </summary>
        public string SOURCEKIND { get; set; }

        /// <summary>
        /// 預算類別
        /// </summary>
        public string BUDGETTYPE { get; set; }

        /// <summary>
        /// 預算項目
        /// </summary>
        public string PLANITEMC { get; set; }

        /// <summary>
        /// 中央補助款(千元)
        /// </summary>
        public int BUDGETCENTRAL { get; set; }

        /// <summary>
        /// 地方自籌款(千元)
        /// </summary>
        public int BUDGETLOCAL { get; set; }

    }
}
