using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Words;
using SDO.Base.RPT.Enums;
using SDO.Dac;
using SDO.Services;

namespace SDO.ReportBuilder.Models
{
    /// <summary>
    /// 報表參數
    /// </summary>
    public class RptBaseParameter
    {
        /// <summary>
        /// 報表ID
        /// </summary>
        public string ReportId { get; set; }

        /// <summary>
        /// 報表型態
        /// </summary>
        public ReportTypeEnum ReportType { get; set; }

        /// <summary>
        /// 檔名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 副檔名
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// 紙張大小
        /// </summary>
        public PaperSize paperSize { get; set; } = PaperSize.A4;

        /// <summary>
        /// 版面是否為直式 false:橫式
        /// </summary>
        public bool OrientationVirtical { get; set; } = true;

        public Orientation PrintOri
        {
            get
            {
                return OrientationVirtical ? Orientation.Portrait : Orientation.Landscape;
            }
        }

        /// <summary>
        /// 存檔路徑不含檔名
        /// </summary>
        public string SavePath { get; set; }

    }
}
