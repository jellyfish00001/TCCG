using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.IPC.Models.Statistics
{
    public class ProjectAdjustDetailedModel
	{
		/// <summary>
		/// 調整流水號
		/// </summary>
		public int PROJ_ADJ_ID { get; set; }
		/// <summary>
		/// 列管編號
		/// </summary>
		public string PROJECT_NO { get; set; }
		/// <summary>
		/// 計畫名稱
		/// </summary>
		public string PROJECT_NAME { get; set; }
		/// <summary>
		/// 原執行情形
		/// </summary>
		public string RUNWAY_C_ORI { get; set; }
		/// <summary>
		/// 執行情形
		/// </summary>
		public string RUNWAY_C { get; set; }
		/// <summary>
		/// 執行機關中文簡寫
		/// </summary>
		public string EXEC_ORGAN_NAME { get; set; }
		/// <summary>
		/// 總預算經費
		/// </summary>
		public double TOTAL_BUDGET { get; set; }
		/// <summary>
		/// 異動紀錄ID
		/// </summary>
		public int LOG_ID { get; set; }
		/// <summary>
		/// 期程調整歷程
		/// </summary>
		public List<AdjustScheHistoryModel> HistoryModels { get; set; }
		/// <summary>
		/// 自訂檢核點資料
		/// </summary>
		public List<ProjectCusCheckpointModel> CusChkpt { get; set; }
		/// <summary>
		/// 調整時的自訂檢核點資料
		/// </summary>
		public List<AdjustCusCheckPointModel> AdjustCusChkpt { get; set; }
	}
}
