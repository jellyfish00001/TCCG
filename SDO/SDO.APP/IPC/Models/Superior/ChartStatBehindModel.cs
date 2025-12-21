using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
	/// <summary>
	/// 落後統計圖
	/// </summary>
	public class ChartStatBehindModel
    {
		/// <summary>
		/// 計畫編號
		/// </summary>
		public string PROJECT_NO { get; set; }

		/// <summary>
		/// 落後類別
		/// </summary>
		public string DELAY_KIND { get; set; }

		/// <summary>
		/// 落後次類別代碼
		/// </summary>
		public string DELAY_SUBCLASS_C { get; set; }

		/// <summary>
		/// 落後次類別名稱
		/// </summary>
		public string DELAY_CLASS_SUB_ITEM { get; set; }
	}
}
