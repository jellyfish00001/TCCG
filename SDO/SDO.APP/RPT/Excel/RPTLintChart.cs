using Aspose.Cells;
using Aspose.Cells.Charts;
using Autofac;
using Renci.SshNet;
using SDO.Models;
using SDO.ReportBuilder.Services;
using SDO.Services;
using System.Drawing;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Excel
{
    /// <summary>
    /// 各年度提案數統計表
    /// </summary>
    public class RPTLintChart : XlsBuilder
    {
        private IInnStatisticsDac statisticsDac;
        private StatisticsModel statistics;

        public RPTLintChart(IComponentContext coms) : base(coms)
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
                // 工作表名稱
                Sheet.Name = $"{lineChartModel.ColumnName}圖";

                Cells cells = Sheet.Cells;

                // 在 A1, B1, C1 寫 "欄", "列", "值"
                cells["A1"].PutValue("欄");
                cells["B1"].PutValue("列");
                cells["C1"].PutValue("值");

                // 欄
                int row = 2;
                SetColumn(cells[$"A{row}"], lineChartModel.ChartColumn);

                // 列
                row = 2;
                foreach (string item in lineChartModel.ChartRow)
                {
                    SetColumn(cells[$"B{row}"], item);
                    row++;
                }

                // 值
                row = 2;
                foreach (decimal item in lineChartModel.ChartValue1)
                {
                    SetColumn(cells[$"C{row}"], item);
                    row++;
                }

                // 生成摺線圖
                CreateLineChart(lineChartModel, chartIndex, 2, row - 1);

                chartIndex++;
            }
        }

        /// <summary>
        /// 生成摺線圖
        /// </summary>
        private void CreateLineChart(LineChartModel lineChartModel, int chartIndex, int startRow, int endRow)
        {
            // 創建摺線圖
            int chartIndexInSheet = Sheet.Charts.Add(ChartType.Line, 1, 5, 25, 19);
            Chart chart = Sheet.Charts[chartIndexInSheet];

            // 圖表的標題 樣式
            chart.Title.Text = lineChartModel.ColumnName;
            chart.Title.Font.IsBold = true;
            chart.Title.Font.Size = 12;

            // X Y 軸標題
            chart.CategoryAxis.Title.Text = "月份";
            chart.CategoryAxis.Title.Font.IsBold = true;

            chart.ValueAxis.Title.Text = "百分比 (%)";
            chart.ValueAxis.Title.Font.IsBold = true;

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
            // 刻度區間
            chart.ValueAxis.MajorUnit = 20;

            // 設定線條樣式
            Series series = chart.NSeries[0];
            chart.NSeries[0].Name = "落後比率";
            chart.NSeries[0].Line.Weight = Aspose.Cells.Drawing.WeightType.MediumLine;
            chart.NSeries[0].Line.Color = Color.Red;

            // 節點樣式
            chart.NSeries[0].Marker.MarkerStyle = ChartMarkerType.Circle; // 空心圓
            chart.NSeries[0].Marker.Border.Color = Color.Red; // 邊框顏色
            chart.NSeries[0].Marker.Area.ForegroundColor = Color.White; // 填充白色
            chart.NSeries[0].Marker.Area.Formatting = FormattingType.Custom; // 樣式

            // 顯示節點上數據
            series.DataLabels.ShowValue = true; // 顯示數據
            series.DataLabels.TextFont.IsBold = false; // 字體樣式
            series.DataLabels.TextFont.Size = 10; // 字體大小
            series.DataLabels.Position = LabelPositionType.Above; // 位置在節點之上
            series.DataLabels.TextFont.IsBold = true; // 粗體


            // 圖表背景樣式
            chart.PlotArea.Area.Formatting = FormattingType.Automatic;
            chart.ChartArea.Area.Formatting = FormattingType.Automatic;

            // Y 軸（值軸）網格線樣式
            chart.ValueAxis.MajorGridLines.IsVisible = true;
            chart.ValueAxis.MajorGridLines.Color = Color.LightGray;

            // X 軸（類別軸）網格線樣式，增加橫向網格線
            chart.CategoryAxis.MajorGridLines.IsVisible = true;
            chart.CategoryAxis.MajorGridLines.Color = Color.LightGray;

        }
    }
}
