using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class BudgetExecModel : DbEditor
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PLANNO { get; set; }

        /// <summary>
        /// (執行)年度
        /// </summary>
        public int EXEYEAR { get; set; }

        /// <summary>
        /// 預算成長幅度%
        /// </summary>
        public decimal GROWRATIO { get; set; }

        /// <summary>
        /// 預算執行數(千元)
        /// </summary>
        public int? EXECOUNT { get; set; }

        /// <summary>
        /// 預算執行率%
        /// </summary>
        public decimal RATIO { get; set; }

        /// <summary>
        /// 法定預算數(千元)包含公務及基金
        /// </summary>
        public int? PUBLICMONEY { get; set; }

        /// <summary>
        /// 預算執行說明
        /// </summary>
        public string EXEDESC { get; set; }

        /// <summary>
        /// 有無歷年預算執行率
        /// </summary>
        public int NOBUDGETYN { get; set; }

        /// <summary>
        /// 無歷年預算執行率原因
        /// </summary>
        public string NOBUDGETDESC { get; set; }

    }
}
