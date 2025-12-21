using Aspose.Cells;
using Autofac;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Excel
{
    public class RPTIPCProjectBudgetExec : XlsBuilder
    {
        private IStatisticsDac statisticsDac;
        private StatisticsModel statistics;
        private List<IPCProjectBudgetExecModel> data;

        public RPTIPCProjectBudgetExec(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "IPCProjectBudgetExecRPT.xlsx";
            statisticsDac = coms.Resolve<IStatisticsDac>();
        }

        protected override async Task GetData()
        {
            StatisticsModel model = Parameter.ObjectModel is null ? new() : (StatisticsModel)(Parameter.ObjectModel);
            statistics = model;
            data = await statisticsDac.GetIPCProjectBudgetExec(model);
        }

        protected override void MakeContent()
        {
            Sheet.Name = $"{statistics.YEAR_MONTH_START.ToTwDateString("yyy-MM")}-明細表(彙整)";

            Cells cells = Sheet.Cells;

            SetColumn(cells["A2"], $"截至{statistics.STATISTICS_YEAR}年{statistics.STATISTICS_MONTH:00}月止");

            int strRow = 4;
            int row = strRow;
            int index = 1;
            foreach (IPCProjectBudgetExecModel item in data)
            {
                if (index > 1)
                {
                    cells.InsertRow(row - 1);
                }

                SetColumn(cells[$"A{row}"], index);
                SetColumn(cells[$"B{row}"], item.MASTER_ORGAN_NAME);
                SetColumn(cells[$"C{row}"], item.EXEC_ORGAN_NAME);
                SetColumn(cells[$"D{row}"], item.PROJECT_NAME);
                SetColumn(cells[$"E{row}"], item.SCHEDULE);
                SetColumn(cells[$"F{row}"], Round(item.PROJECT_EXS));
                SetColumn(cells[$"G{row}"], Round(item.GT_EXPANDED_BUDGET));
                SetColumn(cells[$"H{row}"], Round(item.GT_TOTAL));
                SetColumn(cells[$"I{row}"], item.GT_EXEC_RATE / 100);
                SetColumn(cells[$"J{row}"], Round(item.YEAR_BUDGET_EXPANDED));
                SetColumn(cells[$"K{row}"], Round(item.YEAR_BUDGET_ALLOCATED));
                SetColumn(cells[$"L{row}"], Round(item.YEAR_EXEC_BUDGET));
                SetColumn(cells[$"M{row}"], item.YEAR_EXEC_RATE / 100);
                SetColumn(cells[$"N{row}"], item.IPCBGTEXECFAILED);
                SetColumn(cells[$"O{row}"], item.IPCBGTEXECFAILEDDUTY);
                SetColumn(cells[$"P{row}"], item.EXEC_RATE_FAILED_NOTE);
                SetColumn(cells[$"Q{row}"], item.PCC_PROJECT_NO);
                SetColumn(cells[$"R{row}"], item.FACTORY_CONTACT);
                row++;
                index++;
            }

            // 小計
            int endRow = strRow + data.Count;
            int subtotalRow = endRow + 1;
            List<string> enColumns = new List<string> { "F", "G", "H", "J", "K", "L" };
            foreach (string enColumn in enColumns)
            {
                SetFormula(cells[$"{enColumn}{subtotalRow}"], $"=SUM({enColumn}{strRow}:{enColumn}{endRow})");
            }

            SetFormula(cells[$"I{subtotalRow}"], $"=IF(G{subtotalRow}=0,0,H{subtotalRow}/G{subtotalRow})");
            SetFormula(cells[$"M{subtotalRow}"], $"=IF(K{subtotalRow}=0,0,L{subtotalRow}/K{subtotalRow})");
        }

        private decimal Round(decimal value)
        {
            return Math.Round(value / 1000, 0, MidpointRounding.AwayFromZero);
        }
    }
}
