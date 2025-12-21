using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫章節表 QueryModel
    /// </summary>
    public class ProjectChapterQueryModel 
    {
        /// <summary>
        /// 計畫作業
        /// </summary>
        public string OPERATION { get; set; }
        /// <summary>
        /// 使用角色
        /// </summary>
        public int USER_ROLE { get; set; }
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 系統代號
        /// </summary>
        public string ApId { get; set; }
    }
}
