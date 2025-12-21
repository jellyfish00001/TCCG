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
    public class RPTMajorProjectManageStatus : XlsBuilder
    {
        private IStatisticsDac statisticsDac;
        private StatisticsModel statistics;
        private List<MajorProjectManageStatusModel> data;

        public RPTMajorProjectManageStatus(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "MajorProjectManageStatus.xlsx";
            statisticsDac = coms.Resolve<IStatisticsDac>();
        }

        protected override async Task GetData()
        {
            StatisticsModel model = (StatisticsModel)Parameter.ObjectModel;
            statistics = model;
            data = await statisticsDac.GetMajorProjectManageStatus(model);
        }

        protected override void MakeContent()
        {
            Cells cells = Sheet.Cells;
            Sheet.Name = "重大建設計畫列管情形";

            CellReplaceByExcel(new
            {
                YEAR = statistics.DAB_YEAR_YYY,
                MONTH = statistics.MONTH
            });

            int row = 3;
            int index = 1;
            foreach (MajorProjectManageStatusModel item in data)
            {
                if (index > 1)
                {
                    cells.InsertRow(row - 1);
                }
                SetColumn(cells[$"A{row}"], index);
                SetColumn(cells[$"B{row}"], item.PROJECT_NO);
                SetColumn(cells[$"C{row}"], item.PROJECT_NAME);
                SetColumn(cells[$"D{row}"], item.STAGE);
                SetColumn(cells[$"E{row}"], item.EXEC_DEPT);
                SetColumn(cells[$"F{row}"], item.BUDGET_TOTAL);
                SetColumn(cells[$"G{row}"], item.TOWNNAME);
                SetColumn(cells[$"H{row}"], item.MAIN_CONSTRUCTION);
                SetColumn(cells[$"I{row}"], item.SUBSIDIARY_FACILITY);
                SetColumn(cells[$"J{row}"], item.DELAY_TYPE);
                SetColumn(cells[$"K{row}"], item.CLOSURE_CANCELLATION_DATE.ToTwDateString());
                row++;
                index++;
            }

        }

    }
}
