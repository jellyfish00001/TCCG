using Aspose.Cells;
using Autofac;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Services;
using SDO.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Excel
{
    /// <summary>
    /// 各年度提案數統計表
    /// </summary>
    public class RPTYearStatistics : XlsBuilder
    {
        private IInnStatisticsDac statisticsDac;
        private InnStatisticsModel statistics;
        private List<YearStatisticsModel> data;

        public RPTYearStatistics(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "RPTYearStatistics.xlsx";
            statisticsDac = coms.Resolve<IInnStatisticsDac>();
        }

        protected override async Task GetData()
        {
            InnStatisticsModel model = (InnStatisticsModel)Parameter.ObjectModel;
            statistics = model;
            data = await statisticsDac.GetYearStatistics(model);
        }

        protected override void MakeContent()
        {
            Cells cells = Sheet.Cells;
            Sheet.Name = "各年度提案數統計表";

            CellReplaceByExcel(new
            {
                INN_YEAR = statistics.INN_YEAR
            });

            int row = 3;
            int index = 1;
            int sum = 0; //累計
            foreach (YearStatisticsModel item in data)
            {
                if (index > 1)
                {
                    cells.InsertRow(row - 1);
                }
                SetColumn(cells[$"A{row}"], item.INN_YEAR + "年");
                SetColumn(cells[$"B{row}"], item.PLAN_SUM);
                SetColumn(cells[$"C{row}"], sum += item.PLAN_SUM);
                SetColumn(cells[$"D{row}"], item.COUNT_SPONSOR_ORG);
                SetColumn(cells[$"E{row}"], item.COUNT_SPONSOR_DISTRICT_OFFICE);
                SetColumn(cells[$"F{row}"], item.COUNT_SPONSOR_TYPE_1);
                SetColumn(cells[$"G{row}"], item.COUNT_SPONSOR_TYPE_2);
                SetColumn(cells[$"H{row}"], item.COUNT_SPONSOR_SEX_1);
                SetColumn(cells[$"I{row}"], item.COUNT_SPONSOR_SEX_2);
                row++;
                index++;
            }

        }

    }
}
