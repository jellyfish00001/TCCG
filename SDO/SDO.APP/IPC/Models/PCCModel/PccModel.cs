using SDO.APP.IPC.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class PccModel
    {
        /// <summary>
        /// 資料表名稱
        /// </summary>
        public PccmNameEnum TableName { get; set; }

        /// <summary>
        /// 篩選
        /// </summary>
        public PccFilterModel PccFilter { get; set; }

        /// <summary>
        /// 欄位清單
        /// </summary>
        public List<string> Columns { get; set; }

        /// <summary>
        /// 排序規則
        /// </summary>
        public Dictionary<string, PccmOrderByEnum> Orderby { get; set; }
    }
}
