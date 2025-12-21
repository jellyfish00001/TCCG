using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
	/// <summary>
	/// 決策計畫
	/// </summary>
	public class DecisionPlanModel
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
		/// 金額
		/// </summary>
		public decimal PROJ_BUDGET { get; set; }

		/// <summary>
		/// 進度落差
		/// </summary>
		public int PRG_OFFSET { get; set; }

		/// <summary>
		/// 進度落差描述
		/// </summary>
		public string PRG_OFFSET_DESC
		{
			get
			{
				return PRG_OFFSET <= -5 ? "落後>=5%"
					: -5 < PRG_OFFSET && PRG_OFFSET < 0 ? "落後<5%"
					: PRG_OFFSET >= 0 ? "符合進度"
					: string.Empty;
			}
		}

		/// <summary>
		/// 主管機關名稱
		/// </summary>
		public string MASTER_ORGAN_NAME { get; set; }

		/// <summary>
		/// 執行機關名稱
		/// </summary>
		public string EXEC_ORGAN_NAME { get; set; }
	}
}
