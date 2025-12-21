using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 落後案件
    /// </summary>
    public class DelayPlanModel
    {
        /// <summary>
		/// 關心個案
		/// </summary>
		public bool PIS_SELECT { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
		/// 計畫名稱
		/// </summary>
		public string PROJECT_NAME { get; set; }

        /// <summary>
		/// 主管機關名稱
		/// </summary>
		public string MASTER_ORGAN_NAME { get; set; }

        /// <summary>
        /// 執行機關名稱
        /// </summary>
        public string EXEC_ORGAN_NAME { get; set; }

        /// <summary>
        /// 落後原因
        /// </summary>
        public string OFFSET_REMARK { get; set; }

        /// <summary>
        /// 工程階段
        /// </summary>
        public string ENGNEER_STAGE { get; set; }

        /// <summary>
        /// 工程進度落差
        /// </summary>
        public int ENG_PRG_OFFSET { get; set; }
        /// <summary>
		/// 檢核點進度落差
		/// </summary>
		public int PRG_OFFSET { get; set; }
        /// <summary>
        /// 落後類型
        /// </summary>
        public string DELAY_KIND { get; set; }

    }
}
