using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 工程標案基本資料 (工程會API欄位)
    /// </summary>
    public class PccmDs01Model
    {
        /// <summary>
        /// 標案uid
        /// </summary>
        public string plnprj_uid { get; set; }

        /// <summary>
        /// 標案id
        /// </summary>
        public string plnprj_id { get; set; }

        /// <summary>
        /// 標案名稱
        /// </summary>
        public string plnprj_name { get; set; }

        /// <summary>
        /// 聯絡人
        /// </summary>
        public string contact_name { get; set; }

        /// <summary>
        /// 連絡電話
        /// </summary>
        public string contact_tel { get; set; }

        /// <summary>
        /// 決標金額
        /// </summary>
        public decimal? budget { get; set; }

        /// <summary>
        /// 工程會預定開工日期
        /// </summary>
        public DateTime? scheduledstartdate { get; set; }

        /// <summary>
        /// 工程會預定竣工日期
        /// </summary>
        public DateTime? scheduledenddate { get; set; }

        /// <summary>
        /// 工程會實際開工日期
        /// </summary>
        public DateTime? actualstartdate { get; set; }

        /// <summary>
        /// 工程會實際竣工日期
        /// </summary>
        public DateTime? actualenddate { get; set; }

        /// <summary>
        /// 資料更新時間
        /// </summary>
        public DateTime? updatetime { get; set; }

        /// <summary>
        /// 變更設計後預定完工日期
        /// </summary>
        public DateTime? scheduledenddate_f { get; set; }
    }
}
