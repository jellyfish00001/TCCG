using SDO.Base.Utils.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class ProjectTitleModel : DbEditor
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 創新提案年度
        /// </summary>
        public string YEAR { get; set; }

        /// <summary>
        /// 類別代碼
        /// </summary>
        public string CODE { get; set; }

        /// <summary>
        /// 類別名稱
        /// </summary>
        public string CODE_VALUE { get; set; }

        /// <summary>
        /// 是否涉及其他提案
        /// </summary>
        public bool IS_PLURAL { get; set; }

        /// <summary>
        /// 建立日期
        /// </summary>
        public DateTime? CRT_DATE { get; set; }

        /// <summary>
        /// 最後更新日期
        /// </summary>
        public DateTime? MDF_DATE { get; set; }

    }
}
