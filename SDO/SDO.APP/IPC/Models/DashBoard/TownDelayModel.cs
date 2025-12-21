using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 行政區落後情形統計資料
    /// </summary>
    public class TownDelayModel
    {
        /// <summary>
        /// 行政區代碼
        /// </summary>
        public string TOWN_C { get; set; }
        /// <summary>
        /// 行政區名稱
        /// </summary>
        public string TOWNNAME { get; set; }
        /// <summary>
        /// 工程進度落後未達10%
        /// </summary>
        public int EngDelayBelow10Cnt { get; set; }
        /// <summary>
        /// 工程進度落後10%以上，未達20%
        /// </summary>
        public int EngDelayBelow20Cnt { get; set; }
        /// <summary>
        /// 工程進度落後20%以上
        /// </summary>
        public int EngDelayOver20Cnt { get; set; }
        /// <summary>
        /// 檢核點落後未達3個月
        /// </summary>
        public int ChkPtDelayBelow3MonthsCnt { get; set; }
        /// <summary>
        /// 檢核點落後3個月以上，6個月以下
        /// </summary>
        public int ChkPtDelayBelow6MonthsCnt { get; set; }
        /// <summary>
        /// 檢核點落後6個月以上
        /// </summary>
        public int ChkPtDelayOver6MonthsCnt { get; set; }
    }
}
