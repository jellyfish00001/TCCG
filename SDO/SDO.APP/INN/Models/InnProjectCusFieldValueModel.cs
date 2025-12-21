using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 自定義欄位
    /// </summary>
    public class InnProjectCusFieldValueModel : DbEditor
    {
        /// <summary>
        /// 提案編號
        /// </summary>
        public string INN_PLAN_NO { get; set; }

        /// <summary>
        /// 設定序號
        /// </summary>
        public int CUS_FIELD_ID { get; set; }

        /// <summary>
        /// 輸入值
        /// </summary>
        public string CUS_FIELD_VALUE { get; set; }

    }
}
