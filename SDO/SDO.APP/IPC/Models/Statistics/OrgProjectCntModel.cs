using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 機關列管件數Model
    /// </summary>
    public class OrgProjectCntModel
    {
        /// <summary>
        /// 機關代碼
        /// </summary>
        public string OrgId { get; set; }

        /// <summary>
        /// 機關名稱
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// 機關排序
        /// </summary>
        public string OU_SORT_ORDER { get; set; }
        /// <summary>
        /// 列管件數
        /// </summary>
        public int ProjectCnt { get; set; }
    }
}
