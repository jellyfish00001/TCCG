
using System.ComponentModel;

namespace SDO.Models
{
    /// <summary>
    /// 郵件替換參數資料Model
    /// </summary>
    public class RDMailTemplateParamModel
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        [DisplayName("計畫編號")]
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        [DisplayName("計畫名稱")]
        public string PLANNAME { get; set; }

        /// <summary>
        /// 退回原因
        /// </summary>
        [DisplayName("退回原因")]
        public string BACK_REASON { get; set; }

        /// <summary>
        /// 計畫類別
        /// </summary>
        [DisplayName("計畫類別")]
        public string PLANKIND_NAME { get; set; }

        /// <summary>
        /// 主管機關
        /// </summary>
        [DisplayName("機關名稱")]
        public string OU_NAME { get; set; }


    }
}
