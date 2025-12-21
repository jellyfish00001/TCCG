using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.IPC.Models.ProjectAdjust
{
	/// <summary>
	/// 計畫期程一覽資料
	/// </summary>
    public class ProjectScheOverviewModel
    {
        /// <summary>
        /// 計畫調整識別碼
        /// </summary>
        public string PROJ_ADJ_ID { get; set; }
        /// <summary>
        /// 計畫開始日期
        /// </summary>
        public DateTime? CONTROL_DATE1 { get; set; }
        /// <summary>
        /// 執行方式 (工程類/非工程類)
        /// </summary>
        public string CP_KIND { get; set; }
        /// <summary>
        /// 期程調整類型(Y:總期程調整/M:分月期程調整)
        /// </summary>
        public string SCHE_TYPE { get; set; }
        /// <summary>
        /// 檢核點名稱
        /// </summary>
        public string CHECKITEM_NAME { get; set; }
        /// <summary>
        /// 預計開始日期
        /// </summary>
        public DateTime? ESTIMATED_STARTDATE { get; set; }

        /// <summary>
        /// 預計完成日期
        /// </summary>
        public DateTime? ESTIMATED_ENDDATE { get; set; }
        public string EstimatedEndYM
        {
            get
            {
                if(this.ESTIMATED_ENDDATE.HasValue)
                {
                    return this.ESTIMATED_ENDDATE.Value.ToTwDateString("yyy/MM");
                }
                return null;
            }
        }
        /// <summary>
        /// 實際完成日期
        /// </summary>
        public DateTime? ACTUAL_ENDDATE { get; set; }
        /// <summary>
        /// 特定檢核點
        /// </summary>
        public string CTRL_POINT { get; set; }
        /// <summary>
        /// 項目日期
        /// </summary>
        public DateTime? ITEM_DATE { get; set; }

        private string _ItemDateStr { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ItemDateStr
        {
            get
            {
                return _ItemDateStr ?? ITEM_DATE?.ToTwDateString("yyy/MM/dd") ?? "";
            }
            set { _ItemDateStr = value; }
        }
    }
}
