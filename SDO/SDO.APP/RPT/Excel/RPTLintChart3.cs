using Aspose.Cells;
using Aspose.Cells.Charts;
using Aspose.Cells.Drawing;
using Autofac;
using Renci.SshNet;
using SDO.Models;
using SDO.ReportBuilder.Services;
using SDO.Services;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Excel
{
    /// <summary>
    /// 歷年列管情形統計表
    /// </summary>
    public class RPTLintChart3 : XlsBuilder
    {
        private IInnStatisticsDac statisticsDac;
        private StatisticsModel statistics;

        public RPTLintChart3(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "RPTLintChart.xlsx";
            statisticsDac = coms.Resolve<IInnStatisticsDac>();
        }

        protected override async Task GetData()
        {
            // 將傳遞進來的資料轉為 StatisticsModel
            StatisticsModel model = (StatisticsModel)Parameter.ObjectModel;
            statistics = model;
        }

        protected override void MakeContent()
        {
            // 判斷第幾個圖表
            int chartIndex = 0;

            foreach (var lineChartModel in statistics.LineChartModels)
            {
                // 創建新工作表
                Worksheet sheet = Xls.Worksheets.Add($"{lineChartModel.ColumnName}圖");

                Cells cells = sheet.Cells;

                // 在 A1, B1, C1 寫 "欄", "列", "值"
                cells["A1"].PutValue("欄");
                cells["B1"].PutValue("列");
                cells["C1"].PutValue("值");

                // 欄
                int row = 2;
                SetColumn(cells[$"A{row}"], lineChartModel.ChartColumn);

                // 列、值
                row = 2;
                for (int i = 0; i < lineChartModel.ChartRow.Count; i++)
                {
                    SetColumn(cells[$"B{row}"], lineChartModel.ChartRow[i]);
                    SetColumn(cells[$"C{row}"], lineChartModel.ChartValue1[i]);
                    row++;
                }

                // 是直條還是折線
                if (lineChartModel.ChartType == "column")
                {
                    CreateColumnChart(sheet, lineChartModel, 2, row - 1);
                }
                else if (lineChartModel.ChartType == "line")
                {
                    CreateLineChart(sheet, lineChartModel, 2, row - 1);
                }

                chartIndex++;
            }

            // 刪除多餘工作表
            if (Xls.Worksheets.Count > statistics.LineChartModels.Count)
            {
                Xls.Worksheets.RemoveAt(0);
            }
        }

        /// <summary>
        /// 生成平面直條圖
        /// </summary>
        private void CreateColumnChart(Worksheet sheet, LineChartModel lineChartModel, int startRow, int endRow)
        {
            // 創建平面直條圖
            int chartIndexInSheet = sheet.Charts.Add(ChartType.Column, 1, 5, 25, 19);
            Chart chart = sheet.Charts[chartIndexInSheet];

            // 圖表標題 樣式
            chart.Title.Text = lineChartModel.ColumnName;
            chart.Title.Font.IsBold = true;
            chart.Title.Font.Size = 12;

            // X Y 軸標題水平
            chart.CategoryAxis.Title.Font.IsBold = true;
            chart.CategoryAxis.Title.RotationAngle = 0;

            chart.ValueAxis.Title.Font.IsBold = true;
            chart.ValueAxis.Title.RotationAngle = 0;

            //X Y 軸數據來源，使用動態範圍
            chart.NSeries.Add($"C{startRow}:C{endRow}", true);
            chart.NSeries.CategoryData = $"B{startRow}:B{endRow}";

            // Y 軸範圍
            if (lineChartModel.ColumnStart != lineChartModel.ColumnEnd)
            {
                chart.ValueAxis.MinValue = lineChartModel.ColumnStart;
                chart.ValueAxis.MaxValue = lineChartModel.ColumnEnd;
            }

            // 直條圖樣式
            chart.NSeries[0].Name = lineChartModel.ChartColumn;
            chart.NSeries[0].Area.Formatting = FormattingType.Custom;
            chart.NSeries[0].Area.FillFormat.FillType = FillType.Solid;
            chart.NSeries[0].Area.FillFormat.SolidFill.Color = Color.FromArgb(171, 209, 221);

            // 圖表背景樣式
            chart.PlotArea.Area.Formatting = FormattingType.Automatic;
            chart.ChartArea.Area.Formatting = FormattingType.Automatic;

            //  Y 軸（值軸）網格線樣式
            chart.ValueAxis.MajorGridLines.IsVisible = true;
            chart.ValueAxis.MajorGridLines.Color = Color.LightGray;

            //  X 軸（類別軸）網格線樣式，增加橫向網格線
            chart.CategoryAxis.MajorGridLines.IsVisible = true;
            chart.CategoryAxis.MajorGridLines.Color = Color.LightGray;

            // 數字標籤顯示在直條圖上，加粗體
            chart.NSeries[0].DataLabels.ShowValue = true;
            chart.NSeries[0].DataLabels.Font.IsBold = true;
        }

        /// <summary>
        /// 生成摺線圖
        /// </summary>
        private void CreateLineChart(Worksheet sheet, LineChartModel lineChartModel, int startRow, int endRow)
        {
            // 創建摺線圖
            int chartIndexInSheet = sheet.Charts.Add(ChartType.Line, 1, 5, 25, 19);
            Chart chart = sheet.Charts[chartIndexInSheet];

            // 圖表標題 樣式
            chart.Title.Text = lineChartModel.ColumnName;
            chart.Title.Font.IsBold = true;
            chart.Title.Font.Size = 12;

            // X Y 軸標題水平
            chart.CategoryAxis.Title.Font.IsBold = true;
            chart.CategoryAxis.Title.RotationAngle = 0;
            chart.ValueAxis.Title.Font.IsBold = true;
            chart.ValueAxis.Title.RotationAngle = 0;

            // X Y 軸數據源
            chart.NSeries.Add($"C{startRow}:C{endRow}", true);
            chart.NSeries.CategoryData = $"B{startRow}:B{endRow}";

            // Y 軸範圍
            chart.ValueAxis.MinValue = lineChartModel.ColumnStart;
            chart.ValueAxis.MaxValue = lineChartModel.ColumnEnd;
            chart.ValueAxis.MajorUnit = 20;

            // 線條樣式
            chart.NSeries[0].Name = lineChartModel.ChartColumn;
            chart.NSeries[0].Line.Weight = WeightType.MediumLine;
            chart.NSeries[0].Line.Color = Color.FromArgb(171, 209, 221);
            chart.NSeries[0].Marker.MarkerStyle = ChartMarkerType.Circle;

            // 節點樣式
            chart.NSeries[0].Marker.MarkerStyle = ChartMarkerType.Circle; // 空心圓
            chart.NSeries[0].Marker.Border.Color = Color.FromArgb(171, 209, 221); // 邊框的顏色
            chart.NSeries[0].Marker.Area.ForegroundColor = Color.White; // 填充色白色
            chart.NSeries[0].Marker.Area.Formatting = FormattingType.Custom; // 自訂樣式

            // 圖表背景樣式
            chart.PlotArea.Area.Formatting = FormattingType.Automatic;
            chart.ChartArea.Area.Formatting = FormattingType.Automatic;

            // Y 軸（值軸）網格線樣式
            chart.ValueAxis.MajorGridLines.IsVisible = true;
            chart.ValueAxis.MajorGridLines.Color = Color.LightGray;

            // X 軸（類別軸）網格線樣式，增加橫向網格線
            chart.CategoryAxis.MajorGridLines.IsVisible = true;
            chart.CategoryAxis.MajorGridLines.Color = Color.LightGray;

            // 數字顯示在折線圖上，加粗體
            chart.NSeries[0].DataLabels.ShowValue = true;
            chart.NSeries[0].DataLabels.Font.IsBold = true;
        }
    }
}
