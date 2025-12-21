using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫工程進度新增Model
    /// </summary>
    public class ProjectEngProgressInsertModel : DbEditor
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 重大預定施工進度
        /// </summary>
        public decimal IPC_RES_PRG { get; set; }
        /// <summary>
        /// 重大實際施工進度
        /// </summary>
        public decimal IPC_ACT_PRG { get; set; }
        /// <summary>
        /// 標案預定施工進度
        /// </summary>
        public decimal TEN_RES_PRG { get; set; }

        /// <summary>
        /// 標案實際施工進度
        /// </summary>
        public decimal TEN_ACT_PRG { get; set; }
        /// <summary>
        /// 執行情形說明【對應標案”實際執行摘要”】
        /// </summary>
        public string EXECUTE_CONDITION { get; set; }

        public string YEAR { get; set; }

        public string MONTH { get; set; }
    }
}
