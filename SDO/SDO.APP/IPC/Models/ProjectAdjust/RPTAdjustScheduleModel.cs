using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.IPC.Models.ProjectAdjust
{
	/// <summary>
	/// 期程調整申請表model
	/// </summary>
    public class RPTAdjustScheduleModel
    {
		/// <summary>
		/// 列管編號
		/// </summary>
		public string PROJECT_NO { get; set; }
		/// <summary>
		/// 計畫名稱
		/// </summary>
		public string PROJECT_NAME { get; set; }
		/// <summary>
		/// 執行機關代號
		/// </summary>
		public string EXEC_ORGAN_C { get; set; }
		/// <summary>
		/// 調整類別 ref SET_PARAM.SET_TYPE = 'SCHE_TYPE' (Y:總期程調整、M: 分月調整)
		/// </summary>
		public string SCHE_TYPE { get; set; }
		/// <summary>
		/// 總期程調整次數
		/// </summary>
		public int SCHE_Y_CNT { get; set; }
		/// <summary>
		/// 分月調整次數
		/// </summary>
		public int SCHE_M_CNT { get; set; }
		/// <summary>
		/// 調整次數 (若此次調整為總期程調整，則為SCHE_Y_CNT + 1；若為分月調整，則為SCHE_M_CNT + 1)
		/// </summary>
		public int SCHE_CNT { get; set; }
		/// <summary>
		/// 執行方式類別 (ref SET_PARAM.SET_TYPE = 'CP_KIND')
		/// </summary>
		public string CP_KIND { get; set; }
		/// <summary>
		/// 執行方式 (ref CODE_CHECKPOINT_ITEM.CHECKPOINT_CLASS_ID)
		/// </summary>
		public string RUNWAY_C { get; set; }
		/// <summary>
		/// 調整檔的執行方式類別 (ref SET_PARAM.SET_TYPE = 'CP_KIND')
		/// </summary>
		public string CP_KIND_ADJ { get; set; }
		/// <summary>
		/// 調整檔的執行方式 (ref CODE_CHECKPOINT_ITEM.CHECKPOINT_CLASS_ID)
		/// </summary>
		public string RUNWAY_C_ADJ { get; set; }
		/// <summary>
		/// 基地位置
		/// </summary>
		public string PROJECT_LOCATION { get; set; }
		/// <summary>
		/// 計畫內容
		/// </summary>
		public string ALL_JOB { get; set; }
		/// <summary>
		/// 計畫效益
		/// </summary>
		public string PROJECT_BENEFIT { get; set; }
	}
}
