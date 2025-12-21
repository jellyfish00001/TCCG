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
    public class RPTProjectUnFilledList : XlsBuilder
    {
        private IStatisticsDac statisticsDac;
        private StatisticsModel statistics;
        private List<ProjectUnFilledModel> data;

        public RPTProjectUnFilledList(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "ProjectUnFilledListRPT.xls";
            statisticsDac = coms.Resolve<IStatisticsDac>();
        }

        protected override async Task GetData()
        {
            StatisticsModel model = Parameter.ObjectModel is null ? new() : (StatisticsModel)(Parameter.ObjectModel);

            // 統計年月移除，以每月20號為分隔，以前查上個月，以後查本月。
            DateTime today = DateTime.Today;
            bool isPrevMonth = today.Day < 20;

            model.STATISTICS_YEAR = (isPrevMonth && today.Month == 1) ? today.AddYears(-1).ToTwDateString("yyy"): today.ToTwDateString("yyy");
            model.STATISTICS_MONTH = isPrevMonth ? today.AddMonths(-1).Month : today.Month;

            statistics = model;
            data = await statisticsDac.GetProjectUnFilledList(model);
            if (!data.Any())
            {
                throw new Exception("無檔案可下載");
            }
        }

        protected override void MakeContent()
        {
            Cells cells = Sheet.Cells;

            CellReplaceByExcel(new
            {
                YEAR_MONTH = $"{statistics.STATISTICS_YEAR}年{statistics.STATISTICS_MONTH}月",
                DATE = DateTime.Now.ToTwDateString()
            });

            int row = 4;
            int index = 1;
            foreach (ProjectUnFilledModel item in data)
            {
                if (index > 1)
                {
                    cells.InsertRow(row - 1);
                }

                SetColumn(cells[$"A{row}"], index);
                SetColumn(cells[$"B{row}"], item.PROJECT_NO);
                SetColumn(cells[$"C{row}"], item.PROJECT_NAME);
                SetColumn(cells[$"D{row}"], item.EXEC_ORGAN_NAME);
                SetColumn(cells[$"E{row}"], item.REAL_CONTACT);
                SetColumn(cells[$"F{row}"], item.REAL_TEL);
                SetColumn(cells[$"G{row}"], item.IS_USER_FTY_DATA ? "是" : "否");
                row++;
                index++;
            }
        }
    }
}
