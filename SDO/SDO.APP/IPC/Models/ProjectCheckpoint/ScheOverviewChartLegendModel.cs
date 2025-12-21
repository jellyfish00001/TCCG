using SDO.APP.IPC.Models.ProjectAdjust;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 期程一覽表 - 甘特圖色例Model
    /// </summary>
    public class ScheOverviewChartLegendModel
    {
        /// <summary>
        /// 項目名稱
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 顏色
        /// </summary>
        public Color Color { get; set; }
    }
}
