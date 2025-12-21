using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class GridBasicQryModel
    {
        /// <summary>
        /// 開始日期
        /// </summary>
        [Required]
        public DateTime START_DATE { get; set; }

        /// <summary>
        /// 結束日期
        /// </summary>
        [Required]
        public DateTime END_DATE { get; set; }

        /// <summary>
        /// 查詢頁碼
        /// </summary>
        public int PAGE_NO { get; set; }

        /// <summary>
        /// 每頁幾筆資料
        /// </summary>
        public int PAGE_SIZE { get; set; }
    }
}
