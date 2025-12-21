using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.IPC.Enum
{
    /// <summary>
    /// 群組分群層級
    /// </summary>
    public enum LevelType
    {
        /// <summary>
        /// 父群組類別
        /// </summary>
        Group = 0,
        /// <summary>
        /// 子群組類別
        /// </summary>
        SubGroup = 1,
        /// <summary>
        /// 欄位類別
        /// </summary>
        Field = 2
    }
}
