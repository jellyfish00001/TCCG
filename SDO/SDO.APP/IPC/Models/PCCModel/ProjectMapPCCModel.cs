using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 關聯工程會標案Model
    /// </summary>
    public class ProjectMapPCCModel : DbEditor
    {
        /// <summary>
        /// 標案UID
        /// </summary>
        public string PCC_PROJECT_UID { get; set; }
        /// <summary>
        /// 標案編號
        /// </summary>
        public string PCC_PROJECT_NO { get; set; }
        /// <summary>
        /// 標案名稱
        /// </summary>
        public string PCC_PROJECT_NAME { get; set; }
        /// <summary>
        /// 執行機關
        /// </summary>
        public string PCC_EXEC_ORG_NAME { get; set; }
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 計畫實際承辦人
        /// </summary>
        public string FACTORY_CONTACT { get; set; }

        /// <summary>
        /// 承辦人電話
        /// </summary>
        public string FACTORY_TEL { get; set; }

        /// <summary>
        /// 發包金額
        /// </summary>
        public decimal? PROCUREMENT_AMT { get; set; }

        /// <summary>
        /// 決標金額
        /// </summary>
        public decimal TENDER_AWARDING_AMT { get; set; }
    }
}
