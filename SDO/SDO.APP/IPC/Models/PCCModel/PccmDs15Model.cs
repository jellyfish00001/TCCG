using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 工程標案落後原因 (工程會API欄位)
    /// </summary>
    public class PccmDs15Model
    {
        /// <summary>
        /// 唯一識別碼
        /// </summary>
        public string cpk { get; set; }

        /// <summary>
        /// 標案識別碼
        /// </summary>
        public string plnprj_uid { get; set; }
        /// <summary>
        /// 標案代碼
        /// </summary>
        public string plnprj_id { get; set; }
        /// <summary>
        /// 標案名稱
        /// </summary>
        public string plnprj_name { get; set; }
        /// <summary>
        /// 落後年分
        /// </summary>
        public int? yr { get; set; }
        /// <summary>
        /// 落後月份
        /// </summary>
        public string mnth { get; set; }
        /// <summary>
        /// 落後因素
        /// </summary>
        public string mft { get; set; }
        /// <summary>
        /// 原因分析
        /// </summary>
        public string mfas { get; set; }
        /// <summary>
        ///解決方法
        /// </summary>
        public string mfrt { get; set; }
        /// <summary>
        /// 待協調事項及涉及機關
        /// </summary>
        public string mfru { get; set; }
        /// <summary>
        /// 改進完成期限
        /// </summary>
        public string okdt { get; set; }
        /// <summary>
        /// 責任歸屬
        /// </summary>
        public string respons { get; set; }
        /// <summary>
        /// 資料更新時間
        /// </summary>
        public DateTime? updatetime { get; set; }
    }
}
