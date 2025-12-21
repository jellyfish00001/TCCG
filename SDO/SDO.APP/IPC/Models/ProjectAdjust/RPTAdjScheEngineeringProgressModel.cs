using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.IPC.Models.ProjectAdjust
{
    /// <summary>
    /// 期程調整申請表 - 施工階段說明model
    /// </summary>
    public class RPTAdjScheEngineeringProgressModel
    {
        /// <summary>
        /// 預計完成日期
        /// </summary>
        public DateTime ESTIMATED_ENDDATE { get; set; }
        /// <summary>
        /// 實際完成日期
        /// </summary>
        public DateTime? ACTUAL_ENDDATE { get; set; }
        /// <summary>
        /// 控制點 ref SET_PARAM.SET_ITEM = 'CTRL_CHK_POINT_TYPE'
        /// </summary>
        public string CTRL_POINT { get; set; }
        /// <summary>
        /// 檢核點項目進度
        /// </summary>
        public double PROGRESS { get; set; }
    }
}
