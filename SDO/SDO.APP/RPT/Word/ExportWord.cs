using Aspose.Words;
using Aspose.Words.Drawing.Charts;
using Aspose.Words.Tables;
using Autofac;
using SDO.Base.RPT.Enums;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Word
{
    public class ExportWord : WContentBuilder
    {
        private Dictionary<string, double> data = new Dictionary<string, double>();

        public ExportWord(IComponentContext coms) : base(coms)
        {
        }

        protected override Task GetData()
        {
            data.Add("1班", 1);
            data.Add("2班", 2);
            data.Add("3班", 3);
            data.Add("4班", 4);
            data.Add("5班", 5);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 內容
        /// </summary>
        protected override void Content()
        {
            InsertChart(new WordChartModel
            {
                ChartType = ChartType.Column,
                TitleText = ChartType.Column.ToString(),
                Categories = data.Select(x => x.Key).ToArray(),
                SeriesColls = new List<WordSeriesCollModel>
                {
                    new WordSeriesCollModel
                    {
                        SeriesName = "D1",
                        Values = data.Select(x => x.Value).ToArray(),
                        ShowValue = true,
                        ShowCategoryName = true,
                        Separator = ": ",
                    }
                },
                LegendPosition = LegendPosition.TopRight
            });

            Builder.Writeln(string.Empty);
            Builder.Writeln(string.Empty);

            InsertChart(new WordChartModel
            {
                ChartType = ChartType.Pie,
                TitleText = ChartType.Pie.ToString(),
                Categories = data.Select(x => x.Key).ToArray(),
                SeriesColls = new List<WordSeriesCollModel>
                {
                    new WordSeriesCollModel
                    {
                        Values = data.Select(x => x.Value).ToArray(),
                        ShowValue = true,
                        ShowCategoryName = true,
                        Separator = ": ",
                    }
                },
                LegendPosition = LegendPosition.TopRight
            });

            Builder.Writeln(string.Empty);
            Builder.Writeln(string.Empty);
        }
    }
}
