using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫協辦機關
    /// </summary>
    public class ProjectAsstOrgModel : DbEditor
    {
        /// <summary>
        /// 流水編號
        /// </summary>
        public int ASST_ID { get; set; }

        /// <summary>
        /// 列管編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 協辦機關
        /// </summary>
        public string ASSISTANT_ORGAN_C { get; set; }

        /// <summary>
        /// 協辦機關承辦人/協辦機關人員
        /// </summary>
        public string ASSISTANT_UNDERTAKER_C { get; set; }

        /// <summary>
        /// 協辦機關承辦人/協辦機關人員 名稱
        /// </summary>
        public string ASSISTANT_UNDERTAKER_C_NAME { get; set; }
    }
}
