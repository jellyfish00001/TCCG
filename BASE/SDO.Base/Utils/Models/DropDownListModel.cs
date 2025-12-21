using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Base.Utils.Models
{
    /// <summary>
    /// 下拉清單Model
    /// </summary>
    public class DropDownListModel
    {
        /// <summary>
        /// 顯示的文字
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// 其他參數
        /// </summary>
        public object param { get; set; }
    }
}
