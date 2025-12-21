using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
	/// <summary>
	/// 統計圖
	/// </summary>
	public class ChartStatModel
    {
		public string Value { get; set; }

		/// <summary>
		/// 進度落差
		/// </summary>
		public int PRG_OFFSET { get; set; }

		/// <summary>
		/// 件數
		/// </summary>
        public int Cnt { get; set; }

        /// <summary>
        /// 金額
        /// </summary>
        public decimal PROJ_BUDGET { get; set; }
	}
}
