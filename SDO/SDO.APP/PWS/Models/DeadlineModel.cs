using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDO.Base.Utils.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class DeadlineModel : DbEditor
    {
        /// <summary>
        /// 先期計畫年度
        /// </summary>
        public string PLANYEAR { get; set; }

        /// <summary>
        /// 截止辦理時間
        /// </summary>
        public DateTime? HANDDATEEND { get; set; }

        /// <summary>
        /// 機關截止辦理時間
        /// </summary>
        public DateTime? OrgEndTime { get; set; }

        /// <summary>
        /// 區公所截止辦理時間
        /// </summary>
        public DateTime? DistrictHallEndTime { get; set; }

        /// <summary>
        /// 局處下拉資料
        /// </summary>
        public List<DropDownListModel> dropDownListModels { get; set; }
    }
}
