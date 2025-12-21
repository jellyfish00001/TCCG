using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 下拉清單Model
    /// </summary>
    public class SponsorDropDownListModel
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
        
        /// <summary>
        /// 主要提案人職稱
        /// </summary>
        public string SPONSOR_TITLE { get; set; }

        /// <summary>
        /// 主要提案人所屬單位
        /// </summary>
        public string SPONSOR_UNIT { get; set; }

        public string SPONSOR_ORG { get; set; }
    }
}
