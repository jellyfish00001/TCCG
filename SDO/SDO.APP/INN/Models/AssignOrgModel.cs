using SDO.Base.Utils.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class AssignOrgModel : DbEditor
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public int ASSIGN_ORG_ID { get; set; }

        /// <summary>
        ///創新提案年度
        /// </summary>
        public string INN_YEAR { get; set; }

        /// <summary>
        ///局處代碼
        /// </summary>
        public string OU_ID { get; set; }

        /// <summary>
        /// 截止辦理日期
        /// </summary>
        public DateTime? CLOSE_DATE { get; set; }

        /// <summary>
        /// 是否開放涉及其他提案類別
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
