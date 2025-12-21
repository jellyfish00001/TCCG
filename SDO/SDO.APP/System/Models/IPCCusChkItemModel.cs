using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 自訂檢核點代碼
    /// </summary>
    public class IPCCusChkItemModel : DbEditor
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public int SEQ { get; set; }

        /// <summary>
        /// 執行方式編號
        /// </summary>
        public int CHK_POINT_CLASS_ID { get; set; }

        /// <summary>
        /// 檢核點項目名稱
        /// </summary>
        public string NAME { get; set; }

        /// <summary>
        /// 檢核點項目進度
        /// </summary>
        public int PROGRESS { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IS_ENABLE { get; set; }

        /// <summary>
        /// 控制點
        /// </summary>
        public string CTRL_POINT { get; set; }

        /// <summary>
        /// 所屬政府機關代碼
        /// </summary>
        public string CITY_GOV_ID { get; set; }

        /// <summary>
        /// 刪除註記
        /// </summary>
        public bool DEL_FLG { get; set; }
    }
}
