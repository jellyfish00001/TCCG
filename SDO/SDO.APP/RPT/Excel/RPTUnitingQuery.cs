using Aspose.Cells;
using Autofac;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Excel
{
    public class RPTUnitingQuery : XlsBuilder
    {
        public RPTUnitingQuery(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "UnitingQuery.xlsx";
        }

        protected override Task GetData()
        {
            return Task.CompletedTask;
        }

        protected override void MakeContent()
        {
            var grid = ((ExportGridParameter)Parameter).GridData;

            // 表頭對應欄位
            Dictionary<string, int> columnDict = new Dictionary<string, int>();
            foreach (GridHead header in grid.Header)
            {
                columnDict.Add(header.Field, GetNumColumn(header.Title));
            }

            // 寫入資料
            Cells cells = Sheet.Cells;
            int rowIndex = 2;
            int index = 1;
            // 欲忽略轉換數字的欄位（避免被誤判進行轉換，如：電話、編號、傳真 .. 等）
            string[] ignoreFormatNumberFields = new string[] { "tel", "fax", "plannumber", "name", "planoidnumber", "docid", "usrcustiom1", "plansubnumber", "planitemnumber", "real_tel", "x_coord", "y_coord" };
            string[] percentColumns = new string[] { "GT_EXEC_RATE", "YEAR_EXEC_RATE", "EXACUTIVE_RATE", "IPC_RES_PRG", "IPC_ACT_PRG", "IPC_DIFF_PRG", "TEN_RES_PRG", "TEN_RES_PRG", "TEN_RES_PRG" };

            foreach (Dictionary<string, object> item in grid.Data)
            {
                foreach (string key in item.Keys.Where(x => columnDict.Keys.Contains(x)))
                {
                    int value = columnDict[key];
                    if (value < 0)
                    {
                        continue;
                    }

                    Cell CurrentCell = cells[$"{GetEnColumn(value + 1)}{rowIndex}"];
                    object itemData = item[key] == null ? string.Empty : item[key];

                    Int64 tempInt = 0;
                    double tempDouble = 0;
                    CultureInfo provider = new CultureInfo("en-US");
                    if (Int64.TryParse(itemData.ToString(), NumberStyles.Integer | NumberStyles.AllowThousands, provider, out tempInt))
                    {
                        if (ignoreFormatNumberFields.Contains(key.ToLower()))
                            //不轉為數字
                            WorkbookBuilder.Put(CurrentCell, TextAlignmentType.Left, itemData);
                        else
                            WorkbookBuilder.PutNum(CurrentCell, tempInt);

                    }
                    else if (Double.TryParse(itemData.ToString(), out tempDouble))
                    {
                        if (ignoreFormatNumberFields.Contains(key.ToLower()))
                            //不轉為數字
                            WorkbookBuilder.Put(CurrentCell, TextAlignmentType.Left, itemData);
                        else
                            WorkbookBuilder.PutNum(CurrentCell, tempDouble, numType: 4);
                    }
                    else
                    {
                        if (percentColumns.Contains(key.ToUpper()))
                            WorkbookBuilder.PutNum(CurrentCell, itemData);
                        else
                            WorkbookBuilder.Put(CurrentCell, TextAlignmentType.Left, itemData.ToString().Replace("<br>", "\n"));
                    }

                    WorkbookBuilder.SetBorder(CurrentCell, CellBorderType.Thin, Color.Black);
                }

                WorkbookBuilder.Put(cells[$"A{rowIndex}"], TextAlignmentType.Center, index);
                WorkbookBuilder.SetBorder(cells[$"A{rowIndex}"], CellBorderType.Thin, Color.Black);

                rowIndex++;
                index++;
            }

            // 刪除沒使用的欄位
            // 取得最大表頭列
            int sumRowCount = GetMaxHeaderColumn(0, 0);
            // 不刪除的欄位索引清單
            List<int> notDelColumns = columnDict.Select(x => x.Value).ToList();
            for (int i = sumRowCount - 1; i >= 1; i--)
            {
                if (!notDelColumns.Contains(i))
                {
                    Sheet.Cells.DeleteColumn(i);
                }
            }
        }
    }
}
