using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// Word自動生成Table使用之interface
    /// </summary>
    public interface ITableData
    {
        /// <summary>
        /// 填入表格名稱
        /// </summary>
        string TABLE_NAME { get; set; }

        /// <summary>
        /// 填入表格INDEX
        /// TABLE_NAME優先於TABLE_INDEX
        /// </summary>
        int TABLE_INDEX { get; set; }

        /// <summary>
        /// 範例RowIndex
        /// </summary>
        int CLONE_ROW_INDEX { get; set; }

        /// <summary>
        /// 填入資料
        /// </summary>
        IEnumerable<object> LIST_DATA { get; set; }

        /// <summary>
        /// 自動生成ROW以外之資料
        /// Ex: 金額總和
        /// </summary>
        object COMMOM_DATA { get; set; }
    }
}
