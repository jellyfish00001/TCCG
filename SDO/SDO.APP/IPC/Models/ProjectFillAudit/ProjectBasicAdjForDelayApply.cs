using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 未於期限內提出計畫調整資料
    /// </summary>
    public class ProjectBasicAdjForDelayApply
    {
        /// <summary>
        /// 調整類別
        /// </summary>
        public string SCHE_TYPE { get; set; }

        /// <summary>
        /// 申請日期
        /// </summary>
        public DateTime? APPRV_DATE { get; set; }
    }
}
