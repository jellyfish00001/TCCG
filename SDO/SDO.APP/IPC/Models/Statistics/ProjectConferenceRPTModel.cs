using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 報表4 每月案件落後挑案列表(會議列管資料)
    /// </summary>
    public class ProjectConferenceRPTModel
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 會議種類代碼
        /// </summary>
        public string CONFERENCE_GENRE { get; set; }
        /// <summary>
        /// 會議種類名稱
        /// </summary>
        public string CONFERENCE_NAME { get; set; }
        /// <summary>
        /// 會議次數
        /// </summary>
	    public int CONFERENCE_NUM { get; set; }

        /// <summary>
        /// 會議時間
        /// </summary>
        public DateTime CONFERENCE_TIME{ get; set; }
        /// <summary>
        /// 計畫總經費
        /// </summary>
        public int BUDGET { get; set; }
        /// <summary>
        /// 預定施工進度
        /// </summary>
        public decimal? IPC_RES_PRG { get; set; }
        /// <summary>
        /// 實際施工進度
        /// </summary>
        public decimal? IPC_ACT_PRG { get; set; }

        /// <summary>
        /// 執行情形說明
        /// </summary>
        public string EXECUTE_CONDITION { get; set; }
        /// <summary>
        /// 落後類型
        /// </summary>
        public string DELAY_KIND { get; set; }
        /// <summary>
        /// 落後原因
        /// </summary>
        public string DELAY_CAUSAL { get; set; }

    }
}
