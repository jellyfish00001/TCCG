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
    /// 委託研究計畫執行情形調查表
    /// </summary>
    public class RPTRDPlanExecution : XlsBuilder
    {
        private IRDReportDacDac statisticsDac;
        private ReportQueryModel statistics;
        private List<PlanExecutionModel> data;

        public RPTRDPlanExecution(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "RDPlanExecutionRPT.xlsx";
            statisticsDac = coms.Resolve<IRDReportDacDac>();
        }

        protected override async Task GetData()
        {
            ReportQueryModel model = (ReportQueryModel)Parameter.ObjectModel;
            statistics = model;
            data = await statisticsDac.GetPlanExecution(model);
        }

        protected override void MakeContent()
        {
            Cells cells = Sheet.Cells;
            Sheet.Name = "委託研究計畫執行情形調查表";

            CellReplaceByExcel(new
            {
                YEAR = statistics.PLAN_YEAR,
                SEASON = statistics.SEASON_TYPE
            });

            int row = 5;
            int index = 1;
            foreach (PlanExecutionModel item in data)
            {
                if (index > 1)
                {
                    cells.InsertRow(row - 1);
                }
                SetColumn(cells[$"A{row}"], item.PLAN_YEAR);
                SetColumn(cells[$"B{row}"], item.OU_NAME);
                SetColumn(cells[$"C{row}"], item.PLAN_NAME);
                SetColumn(cells[$"D{row}"], item.ENTRUST_UNIT_NAME);
                SetColumn(cells[$"E{row}"], item.SUM_MONEY != 0 ? $"{item.SUM_MONEY:N0}千元" : null);
                SetColumn(cells[$"K{row}"], item.PLAN_START_DATE.ToTwDateString("yyy年MM月") + "至" + item.PLAN_END_DATE.ToTwDateString("yyy年MM月"));
                SetColumn(cells[$"L{row}"], item.MID_REPORT_YM.ToTwDateString("yyy年MM月"));
                SetColumn(cells[$"N{row}"], item.FINAL_REPORT_YM.ToTwDateString("yyy年MM月"));
                row++;
                index++;
            }

        }

    }
}
