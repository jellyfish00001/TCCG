using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
	/// <summary>
	/// 表9 預算執行情形明細表
	/// </summary>
	public class IPCProjectBudgetExecModel
	{
		/// <summary>
		/// 計畫編號
		/// </summary>
		public string PROJECT_NO { get; set; }

		/// <summary>
		/// 主管機關名稱
		/// </summary>
		public string MASTER_ORGAN_NAME { get; set; }

		/// <summary>
		/// 執行機關名稱
		/// </summary>
		public string EXEC_ORGAN_NAME { get; set; }

		/// <summary>
		/// 計畫名稱
		/// </summary>
		public string PROJECT_NAME { get; set; }

		/// <summary>
		/// 計畫期程-計畫開始日期
		/// </summary>
		public DateTime? CONTROL_DATE1 { get; set; }

		/// <summary>
		/// 計畫期程-預定完成期限
		/// </summary>
		public DateTime? CONTROL_DATE6 { get; set; }

		/// <summary>
		/// 計畫期程
		/// </summary>
		public string SCHEDULE
		{
			get
			{
				return $"{CONTROL_DATE1.ToTwDateString()}\n－\n{CONTROL_DATE6.ToTwDateString()}";
			}
		}

		/// <summary>
		/// 計畫總經費
		/// </summary>
		public decimal PROJECT_EXS { get; set; }

		/// <summary>
		/// 累計預定支用數
		/// </summary>
		public decimal GT_EXPANDED_BUDGET { get; set; }

		/// <summary>
		/// 累計實際支用數
		/// </summary>
		public decimal GT_TOTAL { get; set; }

		/// <summary>
		/// 累計執行率
		/// </summary>
		public decimal GT_EXEC_RATE { get; set; }

		/// <summary>
		/// 本年度可支用預算數
		/// </summary>
		public decimal YEAR_BUDGET_EXPANDED { get; set; }

		/// <summary>
		/// 本年度預算分配數
		/// </summary>
		public decimal YEAR_BUDGET_ALLOCATED { get; set; }

		/// <summary>
		/// 本年度預算執行數
		/// </summary>
		public decimal YEAR_EXEC_BUDGET { get; set; }

		/// <summary>
		/// 本年度執行率
		/// </summary>
		public decimal YEAR_EXEC_RATE { get; set; }

		/// <summary>
		/// 預算執行率未達80%原因
		/// </summary>
		public string IPCBGTEXECFAILED { get; set; }

		/// <summary>
		/// 責任歸屬
		/// </summary>
		public string IPCBGTEXECFAILEDDUTY { get; set; }

		/// <summary>
		/// 說明
		/// </summary>
		public string EXEC_RATE_FAILED_NOTE { get; set; }

		/// <summary>
		/// 工程採購案
		/// </summary>
		public string PCC_PROJECT_NO { get; set; }

		/// <summary>
		/// 聯絡人姓名電話
		/// </summary>
		public string FACTORY_CONTACT { get; set; }

	}
}
