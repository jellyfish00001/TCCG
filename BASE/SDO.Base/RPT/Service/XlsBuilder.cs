using Aspose.Cells;
using Aspose.Cells.Charts;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using SDO.ReportBuilder.Interface;
using SDO.ReportBuilder.Models;
using SDO.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SDO.ReportBuilder.Services
{
    public abstract class XlsBuilder : IRPTBuilder
    {
        protected readonly IWebHostEnvironment webHostEnvironment;
        protected readonly ISetParamService sysParam;

        public XlsBuilder(IComponentContext coms)
        {
            this.webHostEnvironment = coms.Resolve<IWebHostEnvironment>();
            this.sysParam = coms.Resolve<ISetParamService>();
        }

        /// <summary>
        /// 預設文字大小
        /// </summary>
        protected int defaultFontSize = 12;

        /// <summary>
        /// 報表參數
        /// </summary>
        protected RptParameter Parameter { get; set; }

        /// <summary>
        /// excel物件
        /// </summary>
        protected Workbook Xls { get; set; }

        /// <summary>
        /// excel活頁
        /// </summary>
        protected Worksheet Sheet { get; set; }

        /// <summary>
        /// 活頁索引
        /// </summary>
        protected int SheetIndex { get; set; }

        /// <summary>
        /// 樣板名稱
        /// </summary>
        protected string TemplateFileName { get; set; }

        /// <summary>
        /// 範本檔路徑
        /// </summary>
        protected string TemplatePath { get; set; }

        /// <summary>
        /// 建立報表
        /// </summary>
        /// <param name="parameter">報表參數</param>
        /// <returns></returns>
        public virtual async Task<(MemoryStream ms, string outputName, string mime)> Create(RptParameter parameter)
        {
            Parameter = parameter;
            SetTemplateFileName();
            Xls = await GetWorkbook();
            Sheet = Xls.Worksheets[SheetIndex];

            await GetData();
            MakeContent();
            return Save();
        }

        /// <summary>
        /// 取得 excel
        /// </summary>
        /// <returns></returns>
        private async Task<Workbook> GetWorkbook()
        {
            if (string.IsNullOrEmpty(TemplateFileName))
            {
                return new Workbook();
            }

            //取得範本位置
            string contentRootPath = webHostEnvironment.ContentRootPath;
            TemplatePath = (await sysParam.GetSysParam("SystemConfig", "ReportFolder")).SET_VALUE;
            TemplatePath = TemplatePath.TrimStart('~').TrimStart('\\'); //Path.Combine 開頭不可為"\"
            TemplatePath = Path.Combine(contentRootPath, TemplatePath, Parameter.ReportType.ToString(), TemplateFileName);
            return !string.IsNullOrEmpty(TemplatePath) ? new Workbook(TemplatePath) : new Workbook();
        }

        /// <summary>
        /// 設定範本檔案
        /// </summary>
        protected virtual void SetTemplateFileName() { }

        /// <summary>
        /// 存檔
        /// </summary>
        /// <returns></returns>
        private (MemoryStream ms, string outputName, string mime) Save()
        {
            if (Xls == null)
            {
                throw new Exception("報表產生失敗");
            }

            MemoryStream stream = new MemoryStream();
            string mimeType = string.Empty;
            switch (string.IsNullOrEmpty(Parameter.Extension) ? string.Empty : Parameter.Extension.ToLower())
            {
                case "ods":
                    mimeType = UCTableExport.MIME_ODS;
                    Xls.Save(stream, SaveFormat.ODS);
                    break;
                case "pdf":
                    mimeType = UCTableExport.MIME_PDF;
                    Xls.Save(stream, SaveFormat.Pdf);
                    break;
                case "csv":
                    mimeType = UCTableExport.MIME_XLS;
                    Xls.Save(stream, SaveFormat.CSV);
                    break;
                default:
                    mimeType = UCTableExport.MIME_XLS;
                    Parameter.Extension = "xlsx";
                    Xls.Save(stream, SaveFormat.Xlsx);
                    break;
            }
            stream.Position = 0;
            return (stream, $"{Parameter.FileName}.{Parameter.Extension}", mimeType);
        }

        /// <summary>
        /// 建立內容或套表
        /// </summary>
        protected abstract void MakeContent();

        /// <summary>
        /// 取資料
        /// </summary>
        /// <returns></returns>
        protected abstract Task GetData();

        /// <summary>
        /// 設定欄寬
        /// </summary>
        /// <param name="colWidths">欄位寬度列表（未帶此參數則欄位自適化設定大小）</param>
        protected void SetColWidth(List<double> colWidths = null)
        {
            if (colWidths == null || !colWidths.Any())
            {
                Sheet.AutoFitColumns();
                return;
            }

            int index = 0;
            foreach (double colWidth in colWidths)
            {
                Sheet.Cells.SetColumnWidth(index, colWidth);
                index++;
            }
        }

        /// <summary>
        /// 設定高度
        /// </summary>
        /// <param name="colHeights">欄位高度列表（未帶此參數則欄位自適化設定大小）</param>
        protected void SetRowHeight(List<double> colHeights = null)
        {
            if (colHeights == null || !colHeights.Any())
            {
                Sheet.AutoFitRows();
                return;
            }

            int index = 0;
            foreach (double colHeight in colHeights)
            {
                Sheet.Cells.SetRowHeight(index, colHeight);
                index++;
            }
        }

        /// <summary>
        /// 設定欄位
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cell">欄位</param>
        /// <param name="value">值</param>
        /// <param name="style">樣式</param>
        /// <param name="hMergeCnt">水平合併數量</param>
        /// <param name="vMergeCnt">垂直合併數量</param>
        /// <returns></returns>
        protected Cell SetColumn<T>(Cell cell, T value, Style style = null, int? hMergeCnt = null, int? vMergeCnt = null)
        {
            if (!(value is null) && value.ToString().Contains("\n"))
            {
                if (style == null)
                {
                    style = cell.GetStyle();
                }
                style.IsTextWrapped = true;
            }

            if (style != null)
            {
                cell.SetStyle(style);
            }

            cell.PutValue(value);

            if (hMergeCnt.HasValue || vMergeCnt.HasValue)
            {
                int totalRow = vMergeCnt ?? 1;
                int totalColumn = hMergeCnt ?? 1;
                Sheet.Cells.Merge(cell.Row, cell.Column, totalRow, totalColumn);

                for (int row = cell.Row; row < (cell.Row + totalRow); row++)
                {
                    for (int column = cell.Column; column < (cell.Column + totalColumn); column++)
                    {
                        Sheet.Cells[row, column].SetStyle(style);
                    }
                }
            }
            return cell;
        }

        /// <summary>
        /// 設定超連結
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cell">欄位</param>
        /// <param name="value">值</param>
        /// <param name="link">超連結</param> 
        /// <param name="style">樣式</param>
        /// <returns></returns>
        protected Cell SetLink<T>(Cell cell, T value, string link, Style style = null)
        {
            if (style != null)
            {
                cell.SetStyle(style);
            }
            cell.PutValue(value);
            Sheet.Hyperlinks.Add(cell.Name, 1, 1, link);
            return cell;
        }

        /// <summary>
        /// 設定excel公式
        /// </summary>
        /// <param name="cell">欄位</param>
        /// <param name="formula">公式</param>
        /// <param name="style">樣式</param>
        /// <returns></returns>
        protected Cell SetFormula(Cell cell, string formula, Style style = null)
        {
            if (style != null)
            {
                cell.SetStyle(style);
            }
            cell.Formula = formula;
            return cell;
        }

        /// <summary>
        /// 設定下拉式清單
        /// </summary>
        /// <param name="cellArea">欄位區域</param>
        /// <param name="worksheet">活頁</param>
        /// <param name="rangeStartCell">欄位開始範圍</param>
        /// <param name="rangeEndCell">欄位結束範圍</param>
        /// <param name="rangeName">範圍名稱</param>
        protected void SetDropDownList(CellArea cellArea, Worksheet worksheet, string rangeStartCell, string rangeEndCell, string rangeName)
        {
            Validation validation = Sheet.Validations[Sheet.Validations.Add(cellArea)];
            Aspose.Cells.Range validRange1 = worksheet.Cells.CreateRange(rangeStartCell, rangeEndCell);
            validRange1.Name = rangeName;
            validation.Type = ValidationType.List;
            validation.Formula1 = $"={rangeName}";
        }

        /// <summary>
        /// 新增圖檔
        /// </summary>
        /// <param name="model"></param>
        protected void InsertChart(XlsChartModel model)
        {
            // 圖檔位置
            int chartIndex = Sheet.Charts.Add(model.ChartType, model.StartRow, model.StartColumn, model.EndRow, model.EndColumn);

            // 建立圖檔
            Chart chart = Sheet.Charts[chartIndex];

            // 標題
            chart.Title.Font.IsBold = model.Title.TitleFontIsBold;
            if (!string.IsNullOrEmpty(model.Title.TitleText))
            {
                chart.Title.Text = model.Title.TitleText;
            }

            if (model.Title.TitleFontSize > 0)
            {
                chart.Title.Font.Size = model.Title.TitleFontSize;
            }

            // 處裡資料來源
            foreach (var item in model.NSerieses)
            {
                // 新增資料來源
                int nseriesIndex = chart.NSeries.Add(item.DataArea, true);
                // 設定資料來源標題
                chart.NSeries.CategoryData = item.CategoryData;

                Series series = chart.NSeries[nseriesIndex];
                // 設定名子
                series.Name = $"={item.DataName}";
                // 設置顯示值
                series.DataLabels.ShowValue = item.ShowValue;
            }
        }

        /// <summary>
        /// 數字轉換英文欄位
        /// </summary>
        /// <param name="value">數字</param>
        /// <returns></returns>
        protected string GetEnColumn(int value)
        {
            List<string> result = new List<string>();
            SetQuotient(value, result);

            int remainder = value % 26;
            result.Add(remainder == 0 ? "Z" : Convert.ToChar(64 + remainder).ToString());

            return string.Join(string.Empty, result);
        }

        /// <summary>
        /// 取得商數 以26進位
        /// </summary>
        /// <param name="value">傳入值</param>
        /// <param name="result">結果</param>
        /// <returns></returns>
        private int SetQuotient(int value, List<string> result)
        {
            int quotient = value / 26;
            int remainder = value % 26;

            if (remainder == 0)
            {
                quotient--;
            }

            while (quotient > 26)
            {
                quotient = SetQuotient(quotient, result);
            }

            if (quotient > 0)
            {
                result.Add(Convert.ToChar(64 + quotient).ToString());
            }

            return value - quotient * 26;
        }

        /// <summary>
        /// 取得此欄位column值，尋找其所在位置的欄位名稱
        /// </summary>
        /// <param name="columnName">欄位名稱</param>
        /// <param name="rowIndex">要搜尋的行索引</param>
        /// <returns></returns>
        protected int GetNumColumn(string columnName, int rowIndex = 0)
        {
            int result = 0;
            bool isSuccess = false;
            while (!isSuccess)
            {
                object value = Sheet.Cells[rowIndex, result].Value;
                if (value is null)
                {
                    return -1;
                }

                isSuccess = value.ToString() == columnName;
                result = isSuccess ? result : result + 1;
            }
            return result;
        }

        /// <summary>
        /// 取得最大表頭列
        /// </summary>
        /// <param name="rowIndex">行索引</param>
        /// <param name="columnStrIndex">欄開始索引</param>
        /// <returns></returns>
        protected int GetMaxHeaderColumn(int rowIndex, int columnStrIndex)
        {
            int maxHeaderColumn = 1;
            for (int i = columnStrIndex; i < Sheet.Cells.MaxDataColumn; i++)
            {
                if (Sheet.Cells[rowIndex, i].Type == CellValueType.IsNull)
                {
                    maxHeaderColumn = i - 1;
                    break;
                }
                else
                {
                    maxHeaderColumn++;
                }
            }
            return maxHeaderColumn;
        }

        /// <summary>
        /// 欄位部分內容取代
        /// </summary>
        /// <param name="cell">欄位</param>
        /// <param name="basicData">要取得的值</param>
        protected void CellReplace(Cell cell, object basicData)
        {
            if (basicData is null)
            {
                return;
            }

            if (cell.Value is null)
            {
                return;
            }

            string rtnVal = cell.Value.ToString();
            foreach (PropertyInfo property in basicData.GetType().GetProperties())
            {
                object propValue = property.GetValue(basicData);
                if (propValue is null)
                {
                    continue;
                }
                rtnVal = rtnVal.Replace($"#{property.Name}#", propValue.ToString());
            }

            cell.PutValue(rtnVal);
        }

        /// <summary>
        /// 整個Sheet的欄位部分內容取代
        /// </summary>
        /// <param name="basicData">要取得的值</param>
        protected void CellReplaceBySheet(object basicData)
        {
            if (basicData is null)
            {
                return;
            }

            foreach (PropertyInfo property in basicData.GetType().GetProperties())
            {
                object propValue = property.GetValue(basicData);
                if (propValue is null)
                {
                    continue;
                }

                Cell cell = null;
                do
                {
                    string propName = $"#{property.Name}#";
                    cell = Sheet.Cells.Find(propName, cell);
                    if (cell != null)
                    {
                        string rtnVal = cell.Value.ToString().Replace(propName, propValue.ToString());
                        cell.PutValue(rtnVal);
                    }
                }
                while (cell != null);
            }
        }

        /// <summary>
        /// 整個Excel檔的欄位部分內容取代
        /// </summary>
        /// <param name="basicData">要取得的值</param>
        protected void CellReplaceByExcel(object basicData)
        {
            if (basicData is null)
            {
                return;
            }

            foreach (Worksheet sheet in Xls.Worksheets)
            {
                foreach (PropertyInfo property in basicData.GetType().GetProperties())
                {
                    object propValue = property.GetValue(basicData);
                    if (propValue is null)
                    {
                        continue;
                    }

                    Cell cell = null;
                    do
                    {
                        string propName = $"#{property.Name}#";
                        cell = sheet.Cells.Find(propName, cell);
                        if (cell != null)
                        {
                            string rtnVal = cell.Value.ToString().Replace(propName, propValue.ToString());
                            cell.PutValue(rtnVal);
                        }
                    }
                    while (cell != null);
                }
            }
        }

        /// <summary>
        /// excel style設定
        /// </summary>
        protected static class WorkbookBuilder
        {
            public static void Put(Cell cell, TextAlignmentType oriant, object value, Style style = null, bool isTextWrapped = true)
            {
                style = style != null ? style : cell.GetStyle(true);
                style.VerticalAlignment = TextAlignmentType.Center;
                style.HorizontalAlignment = oriant;
                style.IsTextWrapped = isTextWrapped;
                cell.SetStyle(style);
                cell.PutValue(value);
            }

            /// <summary>
            /// 填數字
            /// </summary>
            /// <param name="cell">儲存格</param>
            /// <param name="value">填入值</param>
            /// <param name="numType">格式(預設小數點後兩位有千分位)</param>
            /// <param name="style">樣式</param>
            public static void PutNum(Cell cell, object value, int numType = 3, Style style = null)
            {
                style = style != null ? style : cell.GetStyle(true);
                style.VerticalAlignment = TextAlignmentType.Center;
                style.HorizontalAlignment = TextAlignmentType.Right;
                style.Number = numType;
                cell.SetStyle(style);
                cell.PutValue(value);
            }

            /// <summary>
            /// 填數字
            /// </summary>
            /// <param name="cell">儲存格</param>
            /// <param name="value">填入值</param>
            /// <param name="numType">格式(預設小數點後兩位有千分位)</param>
            /// <param name="style">樣式</param>
            public static void Putfloat(Cell cell, object value, int numType = 4, Style style = null)
            {
                style = style != null ? style : cell.GetStyle(true);
                style.HorizontalAlignment = TextAlignmentType.Right;
                style.Number = numType;
                cell.SetStyle(style);
                cell.PutValue(value);
            }

            /// <summary>
            /// 設定邊界
            /// </summary>
            /// <param name="cell">儲存格</param>
            /// <param name="type">框線類型</param>
            /// <param name="color">顏色</param>
            public static void SetBorder(Cell cell, CellBorderType type, Color color)
            {
                Aspose.Cells.Range range = cell.Worksheet.Cells.CreateRange(cell.Name, cell.Name);
                range.SetOutlineBorders(type, color);
            }

            public static void SetMoneyFormat(Cell cell)
            {
                StyleFlag flag = new StyleFlag { All = true };
                Style style = cell.GetStyle();
                style.Number = 4;
                cell.SetStyle(style, flag);
            }
        }

    }
}
