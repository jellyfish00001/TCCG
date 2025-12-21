using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDO.Models;
namespace SDO.ReportBuilder.Models
{
    /// <summary>
    /// Word自動生成Table使用之interface
    /// </summary>
    public class WordTableData : ITableData
    {
        /// <summary>
        /// 填入表格名稱
        /// </summary>
       public string TABLE_NAME { get; set; }

        /// <summary>
        /// 填入表格INDEX
        /// TABLE_NAME優先於TABLE_INDEX
        /// </summary>
        public int TABLE_INDEX { get; set; }

        /// <summary>
        /// 範例RowIndex
        /// </summary>
        public int CLONE_ROW_INDEX { get; set; }

        /// <summary>
        /// 填入資料
        /// </summary>
        public IEnumerable<object> LIST_DATA { get; set; }

        public List<string> GroupColumn { get; set; }

        public List<string> MergeColumn { get; set; }
        /// <summary>
        /// 自動生成ROW以外之資料
        /// Ex: 金額總和
        /// </summary>
        public object COMMOM_DATA { get; set; }
    }
}
