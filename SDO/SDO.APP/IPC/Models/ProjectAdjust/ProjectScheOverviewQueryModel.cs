using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.IPC.Models.ProjectAdjust
{
	/// <summary>
	/// 計畫期程一覽查詢Model
	/// </summary>
    public class ProjectScheOverviewQueryModel
    {
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 期程查詢類別
        /// Ref.(SET_ITEM = SCHE_TYPE)
        /// </summary>
        public List<string> ScheTypes { get; set; }
    }
}
