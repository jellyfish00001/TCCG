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
    /// 各年度提案數統計表
    /// </summary>
    public class RPTLintChart2 : XlsBuilder
    {
        private IInnStatisticsDac statisticsDac;
        private StatisticsModel statistics;

        public RPTLintChart2(IComponentContext coms) : base(coms)
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

                // "欄", "列", "值"
                cells["A1"].PutValue("欄");
                cells["B1"].PutValue("列");
                cells["C1"].PutValue("值");

                // 欄
                int row = 2;
                SetColumn(cells[$"A{row}"], lineChartModel.ChartColumn);

                // 列、值 (反轉數據順序)
                var reversedChartRow = lineChartModel.ChartRow.AsEnumerable().Reverse().ToList();
                var reversedChartValue1 = lineChartModel.ChartValue1.AsEnumerable().Reverse().ToList();

                row = 2;
                for (int i = 0; i < reversedChartRow.Count; i++)
                {
                    SetColumn(cells[$"B{row}"], reversedChartRow[i]);
                    SetColumn(cells[$"C{row}"], reversedChartValue1[i]);
                    row++;
                }

                // 橫向長條圖
                CreateBarChart(sheet, lineChartModel, 2, row - 1);

                chartIndex++;
            }

            // 刪除多餘工作表
            if (Xls.Worksheets.Count > statistics.LineChartModels.Count)
            {
                Xls.Worksheets.RemoveAt(0);
            }
        }

        /// <summary>
        /// 生成橫向長條圖
        /// </summary>
        private void CreateBarChart(Worksheet sheet, LineChartModel lineChartModel, int startRow, int endRow)
        {
            // 創建橫向長條圖
            int chartIndexInSheet = sheet.Charts.Add(ChartType.Bar, 1, 5, 25, 19);
            Chart chart = sheet.Charts[chartIndexInSheet];

            // 標題和樣式
            chart.Title.Text = lineChartModel.ColumnName;
            chart.Title.Font.IsBold = true;
            chart.Title.Font.Size = 12;

            // 設定 X 軸和 Y 軸標題水平
            chart.CategoryAxis.Title.Font.IsBold = true;
            chart.CategoryAxis.Title.RotationAngle = 0;

            chart.ValueAxis.Title.Font.IsBold = true;
            chart.ValueAxis.Title.RotationAngle = 0;

            // X 軸和 Y 軸數據來源
            chart.NSeries.Add($"C{startRow}:C{endRow}", true); 
            chart.NSeries.CategoryData = $"B{startRow}:B{endRow}";  

            // Y 軸範圍
            chart.ValueAxis.MinValue = lineChartModel.ColumnStart;
            chart.ValueAxis.MaxValue = lineChartModel.ColumnEnd;

            // 條形圖樣式
            chart.NSeries[0].Name = lineChartModel.ChartColumn;
            chart.NSeries[0].Area.Formatting = FormattingType.Custom;
            chart.NSeries[0].Area.FillFormat.FillType = FillType.Solid;
            chart.NSeries[0].Area.FillFormat.SolidFill.Color = Color.FromArgb(255, 83, 83);

            // 圖表背景樣式
            chart.PlotArea.Area.Formatting = FormattingType.Automatic;
            chart.ChartArea.Area.Formatting = FormattingType.Automatic;

            // Y 軸（值軸）網格線樣式
            chart.ValueAxis.MajorGridLines.IsVisible = true;
            chart.ValueAxis.MajorGridLines.Color = Color.LightGray;

            // X 軸（類別軸）網格線樣式，增加橫向網格線
            chart.CategoryAxis.MajorGridLines.IsVisible = true;
            chart.CategoryAxis.MajorGridLines.Color = Color.LightGray;

            // 圖例樣式
            chart.Legend.Position = LegendPositionType.Bottom;
            chart.Legend.Font.Size = 10;

            // 圖表邊框
            chart.ChartArea.Border.IsVisible = true;
            chart.ChartArea.Border.Color = Color.Black;

            // 類別軸次序反轉
            //chart.CategoryAxis.ReverseOrder = true;
        }
    }
}
