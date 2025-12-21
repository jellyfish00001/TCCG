using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 案件分布查詢
    /// </summary>
    public class ProgessQueryModel
    {
        /// <summary>
        /// 主管機關
        /// </summary>
        public string MASTER_DEPT { get; set; }

        /// <summary>
        /// 執行機關
        /// </summary>
        public string EXEC_DEPT { get; set; }

        /// <summary>
        /// 建設類別
        /// </summary>
        public List<string> BUILD_KIND { get; set; } = new List<string>();

        /// <summary>
        /// 是否只有主辦權限
        /// </summary>
        public bool IS_HAND_ROLE { get; set; }

        /// <summary>
        /// 建立者
        /// </summary>
        public string CRT_USER { get; set; }
        /// <summary>
        /// 機關代碼
        /// </summary>
        public string OrgId { get; set; }
    }
}
