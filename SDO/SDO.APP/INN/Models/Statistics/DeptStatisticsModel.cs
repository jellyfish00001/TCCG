using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 表2 各主題機關提案數統計表
    /// </summary>
    public class DeptStatisticsModel
    {
        /// <summary>
        /// 機關別
        /// </summary>
        public string DEPT_TYPE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SPONSOR_ORG { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SPONSOR_UNIT { get; set; }

        /// <summary>
        /// 主題
        /// </summary>
        public string PROPOSALTYPE_MAIN { get; set; }

        public int PROPOSALTYPE_COUNT { get; set; }

        /// <summary>
        /// 序號
        /// </summary>
        public int SQE { get; set; }



    }
}
