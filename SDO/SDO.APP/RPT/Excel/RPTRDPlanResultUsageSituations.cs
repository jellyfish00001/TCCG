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
    /// 本府委託研究計畫成果及運用情形調查列管表
    /// </summary>
    public class RPTRDPlanResultUsageSituations : XlsBuilder
    {
        private IRDReportDacDac statisticsDac;
        private ReportQueryModel statistics;
        private List<PlanResultModel> data;

        public RPTRDPlanResultUsageSituations(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "RDPlanResultUsageSituationsRPT.xlsx";
            statisticsDac = coms.Resolve<IRDReportDacDac>();
        }

        protected override async Task GetData()
        {
            ReportQueryModel model = (ReportQueryModel)Parameter.ObjectModel;
            statistics = model;
            data = await statisticsDac.GetPlanResult(model);
        }

        protected override void MakeContent()
        {
            Cells cells = Sheet.Cells;
            Sheet.Name = "本府委託研究計畫成果及運用情形調查列管表";

            CellReplaceByExcel(new
            {
                YEAR = statistics.PLAN_YEAR,
                SEASON = statistics.SEASON_TYPE
            });

            int row = 6;
            int index = 1;
            foreach (PlanResultModel item in data)
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
                switch (item.SITUATION_TYPE)
                {
                    case "1"://採行
                        SetColumn(cells[$"G{row}"], "V");
                        break;
                    case "2"://參採
                        SetColumn(cells[$"H{row}"], "V");
                        break;
                    case "3": //存查
                        SetColumn(cells[$"I{row}"], "V");
                        break;
                }
                row++;
                index++;
            }

        }

    }
}
