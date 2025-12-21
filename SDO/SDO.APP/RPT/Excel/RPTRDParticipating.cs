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
    /// 參採情形總表
    /// </summary>
    public class RPTRDParticipating : XlsBuilder
    {
        private IRDReportDacDac statisticsDac;
        private ReportQueryModel statistics;
        private List<ParticipatingModel> data;

        public RPTRDParticipating(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "RDParticipatingRPT.xlsx";
            statisticsDac = coms.Resolve<IRDReportDacDac>();
        }

        protected override async Task GetData()
        {
            ReportQueryModel model = (ReportQueryModel)Parameter.ObjectModel;
            statistics = model;
            data = await statisticsDac.GetParticipating(model);
        }

        protected override void MakeContent()
        {
            Cells cells = Sheet.Cells;
            Sheet.Name = "參採情形總表";

            int row = 4;
            int index = 1;
            foreach (ParticipatingModel item in data)
            {
                if (index > 1)
                {
                    cells.InsertRow(row - 1);
                }
                SetColumn(cells[$"A{row}"], item.PLAN_YEAR);
                SetColumn(cells[$"B{row}"], index);
                SetColumn(cells[$"C{row}"], item.OU_NAME);
                SetColumn(cells[$"D{row}"], item.PLAN_NAME);
                SetColumn(cells[$"E{row}"], item.PLAN_START_DATE.ToTwDateString("yyy年MM月") + "至" + item.PLAN_END_DATE.ToTwDateString("yyy年MM月"));
                SetColumn(cells[$"F{row}"], item.SUM_MONEY != 0 ? $"{item.SUM_MONEY:N0}千元" : null);
                SetColumn(cells[$"G{row}"], item.ENTRUST_UNIT_NAME);
                SetColumn(cells[$"H{row}"], item.RESEARCH_NAME);
                SetColumn(cells[$"I{row}"], item.SITUATION_TYPE);
                SetColumn(cells[$"J{row}"], item.SITUATION_DESC);
                row++;
                index++;
            }

        }

    }
}
