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
    public class RPTMajorProjectDelayStatus : XlsBuilder
    {
        private IStatisticsDac statisticsDac;
        private StatisticsModel statistics;
        private List<MajorProjectDelayStatusModel> data;

        public RPTMajorProjectDelayStatus(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "MajorProjectDelayStatus.xlsx";
            statisticsDac = coms.Resolve<IStatisticsDac>();
        }

        protected override async Task GetData()
        {
            StatisticsModel model = (StatisticsModel)Parameter.ObjectModel;
            statistics = model;

            // 獲取年份和月份(轉int)
            int year = int.Parse(model.DAB_YEAR_YYY) + 1911;
            int month = int.Parse(model.DAB_MONTH);

            // 獲取該月份的最後一天
            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            model.STATISTICS_AD_YEAR_MONTH_LAST = lastDayOfMonth;

            data = await statisticsDac.GetMajorProjectDelayStatus(model);
        }

        protected override void MakeContent()
        {
            Cells cells = Sheet.Cells;
            Sheet.Name = "重大建設計畫落後情形";

            CellReplaceByExcel(new
            {
                YEAR = statistics.DAB_YEAR_YYY,
                MONTH = statistics.MONTH
            });

            int row = 3;
            int index = 1;
            foreach (MajorProjectDelayStatusModel item in data)
            {
                if (index > 1)
                {
                    cells.InsertRow(row - 1);
                }
                SetColumn(cells[$"A{row}"], index);
                SetColumn(cells[$"B{row}"], item.PROJECT_NO);
                SetColumn(cells[$"C{row}"], item.PROJECT_NAME);
                SetColumn(cells[$"D{row}"], item.EXEC_DEPT);
                SetColumn(cells[$"E{row}"], item.BUDGET_TOTAL);
                SetColumn(cells[$"F{row}"], item.CENTRAL_SUBSIDY);
                SetColumn(cells[$"G{row}"], item.LOCAL_BUDGET);
                SetColumn(cells[$"H{row}"], item.SCHEDULE_ACTUAL);
                SetColumn(cells[$"I{row}"], item.CHKPT_DELAY_DAYS);
                SetColumn(cells[$"J{row}"], item.ENGINEERING_PROGRESS_IPC_RAD_PRG);
                SetColumn(cells[$"K{row}"], item.DELAY_TYPE);
                SetColumn(cells[$"L{row}"], item.SOLUTION);
                SetColumn(cells[$"M{row}"], item.COORDINATION);
                row++;
                index++;
            }

        }

    }
}
