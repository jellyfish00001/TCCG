using Aspose.Cells;
using SDO.APP.IPC.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class OptionColumnModel
    {
        private string table;

        public OptionColumnModel()
        {
            Type = CellValueType.IsString;
            Level = LevelType.Field;
        }

        /// <summary>
        /// 欄位名稱
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 欄位索引
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 欄位層級
        /// </summary>
        public LevelType Level { get; set; }

        /// <summary>
        /// 上層欄位
        /// </summary>
        public string Parent { get; set; }

        /// <summary>
        /// 欄位是否勾選
        /// </summary>
        public bool IsCheck { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Table
        {
            get
            {
                if (string.IsNullOrEmpty(table))
                {
                    return string.Empty;
                }
                return table;
            }
            set
            {
                table = value;
            }
        }

        /// <summary>
        /// 欄位內容資料型態
        /// </summary>
        public CellValueType Type { get; set; }

        /// <summary>
        /// 欄位寬度
        /// </summary>
        /// <remarks>給予前端 Grid 欄位寬度識別用</remarks>
        public int? Width { get; set; }

        /// <summary>
        /// 畫面上不顯示該欄位
        /// </summary>
        public bool Hidden { get; set; }
    }
}
