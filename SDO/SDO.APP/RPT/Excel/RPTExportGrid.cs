using Aspose.Cells;
using Autofac;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Excel
{
    public class RPTExportGrid : XlsBuilder
    {
        public RPTExportGrid(IComponentContext coms) : base(coms)
        {
        }

        protected override Task GetData()
        {
            return Task.CompletedTask;
        }

        protected override void MakeContent()
        {
            var grid = ((ExportGridParameter)Parameter).GridData;

            Style style = Xls.CreateStyle();
            style.Font.Size = defaultFontSize;

            int columnIndex = 0, rowIndex = 1;
            List<string[]> columnList = new List<string[]>();
            foreach (var item in grid.Header)
            {
                string title = item.Title.Replace("<br>", "\n");
                string field = item.Field.ToString();

                int widthVal = 30;
                if (field.Equals("PROJECT_NAME"))
                {
                    string empty = string.Empty;
                    for (int i = 0; i < ((widthVal - 4) / 2); i++)
                    {
                        empty += "　";
                    }

                    title = $"{empty}{title}{empty}";
                }

                Cell currentCell = Sheet.Cells[0, columnIndex];
                WorkbookBuilder.Put(currentCell, TextAlignmentType.Center, title, style: style, false);
                WorkbookBuilder.SetBorder(currentCell, CellBorderType.Thin, Color.Black);
                columnIndex++;
                columnList.Add(new string[] { field, title });
            }

            // 欲忽略轉換數字的欄位（避免被誤判進行轉換，如：電話、編號、傳真 .. 等）
            string[] ignoreFormatNumberFields = new string[] { "tel", "fax", "plannumber", "name", "planoidnumber", "docid", "usrcustiom1", "plansubnumber", "planitemnumber", "real_tel", "x_coord", "y_coord" };
            string[] percentColumns = new string[] { "GT_EXEC_RATE", "YEAR_EXEC_RATE", "EXACUTIVE_RATE", "IPC_RES_PRG", "IPC_ACT_PRG", "IPC_DIFF_PRG", "TEN_RES_PRG", "TEN_RES_PRG", "TEN_RES_PRG" };

            foreach (Dictionary<string, object> objDic in grid.Data)
            {
                columnIndex = 0;
                foreach (string[] column in columnList)
                {
                    Cell CurrentCell = Sheet.Cells[rowIndex, columnIndex];
                    object itemData = objDic[column[0]] == null ? string.Empty : objDic[column[0]];

                    Int64 tempInt = 0;
                    double tempDouble = 0;
                    CultureInfo provider = new CultureInfo("en-US");
                    if (Int64.TryParse(itemData.ToString(), NumberStyles.Integer | NumberStyles.AllowThousands, provider, out tempInt))
                    {
                        if (ignoreFormatNumberFields.Contains(column[0].ToLower()))
                            //不轉為數字
                            WorkbookBuilder.Put(CurrentCell, TextAlignmentType.Left, itemData, style: style);
                        else
                            WorkbookBuilder.PutNum(CurrentCell, tempInt, style: style);

                    }
                    else if (Double.TryParse(itemData.ToString(), out tempDouble))
                    {
                        if (ignoreFormatNumberFields.Contains(column[0].ToLower()))
                            //不轉為數字
                            WorkbookBuilder.Put(CurrentCell, TextAlignmentType.Left, itemData, style: style);
                        else
                            WorkbookBuilder.PutNum(CurrentCell, tempDouble, numType: 4, style: style);
                    }
                    else
                    {
                        if (percentColumns.Contains(column[0].ToUpper()))
                            WorkbookBuilder.PutNum(CurrentCell, itemData, style: style);
                        else
                            WorkbookBuilder.Put(CurrentCell, TextAlignmentType.Left, itemData.ToString().Replace("<br>", "\n"), style: style);
                    }

                    double rowHeight = 0;
                    string[] lineList = GetLineCount(itemData.ToString().Replace("<br>", "\n"));
                    if (Parameter.Extension == "xls")
                    {
                        int lineCount = lineList.Length + 1;
                        foreach (string eachLineString in lineList)
                        {
                            lineCount += (eachLineString.Length / 30) + 1;
                        }
                        rowHeight = lineCount * (Sheet.Cells.StandardHeight);
                    }
                    else
                    {
                        rowHeight = (lineList.Length + 1) * (Sheet.Cells.StandardHeight + 3);
                    }

                    double headerRowHeight = Sheet.Cells.GetRowHeight(0);
                    Sheet.Cells.SetRowHeight(rowIndex, rowHeight > headerRowHeight ? rowHeight : headerRowHeight);

                    WorkbookBuilder.SetBorder(CurrentCell, CellBorderType.Thin, Color.Black);
                    columnIndex++;
                }
                rowIndex++;
            }
            Sheet.AutoFitColumns();
            Sheet.AutoFitRows();
        }
        private string[] GetLineCount(string lineStr)
        {
            lineStr = lineStr.Replace("\r\n", "\n");
            return lineStr.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
