using Aspose.Words;
using Aspose.Words.Tables;
using Autofac;
using SDO.Models;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Word
{
    public class ExportWordTemplate : WContentBuilder
    {
        public ExportWordTemplate(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "DemoWordTemplate.docx";
        }

        protected override Task GetData()
        {
            List<object> yearList = new();
            List<object> monthList = new();
            for (int i = 0; i < 2; i++)
            {
                yearList.Add(new { LAST_DATE_HISTORY_REASON = "yyy", LAST_COUNT = "LAST_COUNT" });
                monthList.Add(new { EACH_LAST_MONTH_HISTORY_REASON = "mmm", EACH_COUNT = "EACH_COUNT" });
            }

            BasicData = new
            {
                Name = "word範例"
            };

            ListData.Add(new WordTableData { LIST_DATA = yearList });
            ListData.Add(new WordTableData { LIST_DATA = monthList });

            return Task.CompletedTask;
        }

        protected override void Other()
        {
            Table targetTable = (Table)Doc.GetChildNodes(NodeType.Table, true)[0];

            VerticalMergeCells(FindCell(targetTable, "調整歷程"));
            VerticalMergeCells(FindCell(targetTable, "總期程"));
            VerticalMergeCells(FindCell(targetTable, "分月\v期程"));

            List<Cell> lastCountCells = FindCell(targetTable, "LAST_COUNT");
            VerticalMergeCells(lastCountCells, lastCountCells.Count.ToString());

            List<Cell> eachCountCells = FindCell(targetTable, "EACH_COUNT");
            VerticalMergeCells(eachCountCells, eachCountCells.Count.ToString());

        }
    }
}
