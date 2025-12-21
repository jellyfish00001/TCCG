using Aspose.Cells;
using Autofac;
using SDO.APP.RD.Models.Report;
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
    /// 季委託研究計畫列管表
    /// </summary>
    public class RPTRDSeasonReport : XlsBuilder
    {
        private IRDReportDacDac statisticsDac;
        private ReportQueryModel statistics;
        private List<SeasonReportModel> data;

        public RPTRDSeasonReport(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "RDSeasonReportRPT.xlsx";
            statisticsDac = coms.Resolve<IRDReportDacDac>();
        }

        protected override async Task GetData()
        {
            ReportQueryModel model = (ReportQueryModel)Parameter.ObjectModel;
            statistics = model;
            data = await statisticsDac.GetSeason(model);
        }

        protected override void MakeContent()
        {
            Cells cells = Sheet.Cells;
            Sheet.Name = "季委託研究計畫列管表";

            CellReplaceByExcel(new
            {
                YEAR = statistics.PLAN_YEAR,
                SEASON = statistics.SEASON_TYPE
            });

            int row = 5;
            int index = 1;
            foreach (SeasonReportModel item in data)
            {
                if (index > 1)
                {
                    cells.InsertRow(row - 1);
                }
                SetColumn(cells[$"A{row}"], index);
                SetColumn(cells[$"B{row}"], item.PLAN_NAME);
                SetColumn(cells[$"C{row}"], item.OU_NAME);
                SetColumn(cells[$"D{row}"], item.PLAN_START_DATE.ToTwDateString("yyy年MM月"));
                SetColumn(cells[$"E{row}"], item.PLAN_END_DATE.ToTwDateString("yyy年MM月"));
                SetColumn(cells[$"F{row}"], item.SUM_MONEY != 0 ? $"{item.SUM_MONEY:N0}千元" : null);
                SetColumn(cells[$"G{row}"], item.EXECUTION_DESC);
                row++;
                index++;
            }

        }

    }
}
