using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫參數值對應資料Model
    /// </summary>
    public class ProjectMappingDataModel:DbEditor
    {
        /// <summary>
        /// 列管編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 參數類別
        /// </summary>
        public string SET_ITEM { get; set; }

        /// <summary>
        /// 設定代碼
        /// </summary>
        public string SET_TYPE { get; set; }

        /// <summary>
        /// 上傳來源
        /// </summary>
        public string SOURCE_ID { get; set; }
    }
}
