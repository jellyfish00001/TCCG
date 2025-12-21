using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 案件分布
    /// </summary>
    public class ProgessModel
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PROJECT_NAME { get; set; }

        /// <summary>
        /// X座標
        /// </summary>
        public string X_COORD { get; set; }

        /// <summary>
        /// Y座標
        /// </summary>
        public string Y_COORD { get; set; }

        /// <summary>
        /// 辦理地點名稱
        /// </summary>
        public string TOWN_NAME { get; set; }

        /// <summary>
		/// 進度落差
		/// </summary>
		public int PRG_OFFSET { get; set; }

        /// <summary>
        /// 工程階段
        /// </summary>
        public string ENGNEER_STAGE { get; set; }

        /// <summary>
        /// 執行機關名稱
        /// </summary>
        public string EXEC_ORGAN_NAME { get; set; }

        /// <summary>
        /// 總經費
        /// </summary>
        public decimal PROJ_BUDGET { get; set; }
    }
}
