using Aspose.Words;
using Aspose.Words.Tables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.ReportBuilder.Models
{
    /// <summary>
    /// 欄位設定
    /// </summary>
    public class CellConfigModel
    {
        /// <summary>
        /// 背景顏色
        /// </summary>
        public Color BackGroundColor { get; set; } = Color.White;

        /// <summary>
        /// 水平對齊
        /// </summary>
        public ParagraphAlignment Alignment { get; set; }

        /// <summary>
        /// 垂直對齊
        /// </summary>
        public CellVerticalAlignment VerticalAlignment { get; set; }

        /// <summary>
        /// 文字對齊
        /// </summary>
        public bool FontBold { get; set; }

        public double Width { get; set; }

    }
}
