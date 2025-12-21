using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 落後原因類別代碼
    /// </summary>
    public class IPCCodeDelayClassModel : DbEditor
    {
        /// <summary>
        /// 落後原因類別代碼
        /// </summary>
        public string DELAY_CLASS_ID { get; set; }

        /// <summary>
        /// 落後類別
        /// </summary>
        public string DELAY_CLASS_ITEM { get; set; }

        /// <summary>
        /// 落後項目代碼
        /// </summary>
        public string DELAY_CLASS_SUB_ID { get; set; }

        /// <summary>
        /// 舊的落後項目代碼
        /// </summary>
        public string OLD_DELAY_CLASS_SUB_ID { get; set; }

        /// <summary>
        /// 落後項目
        /// </summary>
        public string DELAY_CLASS_SUB_ITEM { get; set; }

        /// <summary>
        /// 所屬政府機關代碼
        /// </summary>
        public string CITY_GOV_ID { get; set; }

        /// <summary>
        /// 是否為系統預設
        /// </summary>
        public bool IS_SYSTEM { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IS_ENABLED { get; set; }

        /// <summary>
        /// 刪除註記
        /// </summary>
        public bool DEL_FLG { get; set; }
    }
}
