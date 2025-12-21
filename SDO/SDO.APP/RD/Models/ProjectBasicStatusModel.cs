using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 委託研究計畫狀態，修改執行類別
    /// </summary>
    public class ProjectBasicStatusModel : DbEditor
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public List<string> PLAN_NOS { get; set; }

        /// <summary>
        /// 執行類別
        /// D 刪除 R 撤銷 L1 送出鎖定L2 解除鎖定
        /// </summary>
        public string EXEC_KIND { get; set; }
    }
}
