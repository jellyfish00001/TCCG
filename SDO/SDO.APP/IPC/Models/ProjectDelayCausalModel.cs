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
    /// 計畫落後原因Model
    /// </summary>
    public class ProjectDelayCausalModel : DbEditor
    {
        /// <summary>
        /// 流水號 Identity
        /// </summary>
        public int SEQ { get; set; }
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 落後項目年度
        /// </summary>
        public string DATA_YEAR { get; set; }
        /// <summary>
        /// 落後項目月份
        /// </summary>
        public string DATA_MONTH { get; set; }
        /// <summary>
        /// 落後原因類型
        /// </summary>
        public string DELAY_KIND { get; set; }
        /// <summary>
        /// 落後原因類別
        /// </summary>
        public string DELAY_CLASS_C { get; set; }
        /// <summary>
        /// 落後原因次類別
        /// </summary>
        public string DELAY_SUBCLASS_C { get; set; }
        /// <summary>
        /// 落後原因
        /// </summary>
        public string DELAY_CAUSAL { get; set; }
        /// <summary>
        /// 解決對策
        /// </summary>
        public string SOLUTION { get; set; }
        /// <summary>
        /// 責任歸屬
        /// </summary>
        public string DELAY_RESPON { get; set; }
        /// <summary>
        /// 須協調事項
        /// </summary>
        public string COORDINATION { get; set; }
        /// <summary>
        /// 改進完成期限
        /// </summary>
        public DateTime? DEADLINES { get; set; }
        /// <summary>
        /// 預定趕上日期
        /// </summary>
        public DateTime? REFINE_DATE { get; set; }

        /// <summary>
        /// 可否存檔
        /// </summary>
        public bool  CanSave { get; set; }

        public bool IS_SEND { get; set; }

        public int YEAR { get; set; }

        public string MONTH { get; set; }
        /// <summary>
        /// 是否取消送出
        /// </summary>
        public bool CancelSend { get; set; }
    }
}
