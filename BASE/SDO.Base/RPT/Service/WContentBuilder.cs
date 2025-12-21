using Aspose.Words;
using Aspose.Words.Drawing.Charts;
using Aspose.Words.Fields;
using Aspose.Words.Tables;
using Autofac;
using SDO.Base.RPT.Enums;
using SDO.ReportBuilder.Interface;
using SDO.ReportBuilder.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.ReportBuilder.Services
{
    public abstract class WContentBuilder : WordService, IContentBuilder
    {

        public WContentBuilder(IComponentContext coms) : base(coms)
        {
        }

        /// <summary>
        /// 注入所需參數
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="parameter"></param>
        public void InjectParameter(DocumentBuilder builder, RptParameter parameter)
        {
            Parameter = parameter;
            Builder = builder;
            Doc = builder.Document;
        }

        /// <summary>
        /// 建立內容
        /// </summary>
        public virtual async Task<bool> MakeContent()
        {
            await GetData();
            Title();
            Header();
            Content();
            Footer();
            return true;
        }

        /// <summary>
        /// 套表
        /// </summary>
        public virtual async Task<bool> MakeContenByTemplate()
        {
            SetTemplateFileName();
            await CreateTemplateDoc();
            await GetData();
            wordSetService.doc = Doc;
            wordSetService.GenWord(BasicData, ListData);
            Other();
            Builder.InsertDocument(Doc, ImportFormatMode.KeepDifferentStyles);
            return true;
        }

        /// <summary>
        /// 取得範本檔
        /// </summary>
        protected virtual async Task<bool> CreateTemplateDoc()
        {
            if (!string.IsNullOrEmpty(TemplateFileName))
            {
                //取得範本位置
                string contentRootPath = webHostEnvironment.ContentRootPath;
                TemplatePath = (await sysParam.GetSysParam("SystemConfig", "ReportFolder")).SET_VALUE;
                TemplatePath = TemplatePath.TrimStart('~').TrimStart('\\'); //Path.Combine 開頭不可為"\"
                TemplatePath = Path.Combine(contentRootPath, TemplatePath, Parameter.ReportType.ToString(), TemplateFileName);
                Doc = !string.IsNullOrEmpty(TemplatePath) ? new Document(TemplatePath) : new Document();
            }
            return true;
        }

        protected abstract Task GetData();

        protected virtual void Title() { }

        protected virtual void Header() { }

        protected virtual void Content() { }

        protected virtual void Footer() { }

        /// <summary>
        /// 套表其他行為
        /// </summary>
        protected virtual void Other() { }

        /// <summary>
        /// 設定範本檔案
        /// </summary>
        protected virtual void SetTemplateFileName() { }

        /// <summary>
        /// 新增連結
        /// </summary>
        /// <param name="displayText">連結文字</param>
        /// <param name="url">連結</param>
        /// <returns>連結文件欄位</returns>
        protected Field InsertLink(string displayText, string url)
        {
            Color oriFontColor = Builder.Font.Color;
            Underline oriUnderline = Builder.Underline;

            Builder.Font.Color = Color.Blue;
            Builder.Font.Underline = Underline.Single;
            Field field = Builder.InsertHyperlink(displayText, url, false);
            Builder.Font.Color = oriFontColor;
            Builder.Font.Underline = oriUnderline;
            return field;
        }

        /// <summary>
        /// 設定 Table 欄位寬度
        /// </summary>
        /// <param name="table">表格</param>
        /// <param name="colWidths">欄位寬度列表（未帶此參數則欄位自適化設定大小）</param>
        /// <param name="allowAutoFit">若是差異比對，需設定為true，否則會跑版</param>
        protected void InitTable(Table table, List<double> colWidths = null, bool allowAutoFit = false)
        {
            if (colWidths == null || !colWidths.Any())
            {
                table.AutoFit(AutoFitBehavior.FixedColumnWidths);
                table.PreferredWidth = PreferredWidth.FromPercent(100);
                return;
            }

            double sum = colWidths.Sum(x => x);

            // 若是差異比對，需設定為true，否則會跑版
            table.AllowAutoFit = allowAutoFit;

            foreach (Row row in table.Rows)
            {
                int index = 0;
                foreach (Cell cell in row.Cells)
                {
                    if (sum <= 100)
                    {
                        // 用百分比設定時，判斷此cell是否是合併的起點
                        if (cell.CellFormat.HorizontalMerge == CellMerge.First)
                        {
                            // 是否第一次跑迴圈
                            bool isFirstTime = true;
                            // 存放合併後的總寬度
                            double mergedWidth = 0;
                            // 從目前cell開始，最多到此row的結尾
                            for (int mergeIdx = index; mergeIdx < row.Cells.Count; mergeIdx++)
                            {
                                // NONE或FIRST且不是第一次迴圈 => 已結束合併
                                if (row.Cells[mergeIdx].CellFormat.HorizontalMerge == CellMerge.None ||
                                    (row.Cells[mergeIdx].CellFormat.HorizontalMerge == CellMerge.First && !isFirstTime))
                                {
                                    break;
                                }
                                // 把之後的寬度都加總至此合併起點
                                mergedWidth += colWidths[mergeIdx];
                                isFirstTime = false;
                            }
                            //合併起始格的資料設定為加總的寬度
                            cell.CellFormat.PreferredWidth = PreferredWidth.FromPercent(mergedWidth);
                        }
                        else
                        {
                            cell.CellFormat.PreferredWidth = PreferredWidth.FromPercent(colWidths[index]);
                        }
                    }
                    else
                    {
                        cell.CellFormat.PreferredWidth = PreferredWidth.FromPoints(colWidths[index]);
                        table.AutoFit(AutoFitBehavior.FixedColumnWidths);
                    }
                    index++;
                }
            }
        }

        /// <summary>
        /// 找某張表格的某欄
        /// </summary>
        /// <param name="table">表格</param>
        /// <param name="key">搜尋字串</param>
        /// <param name="isContains">是否包含</param>
        /// <returns></returns>
        protected List<Cell> FindCell(Table table, string key, bool isContains = true)
        {
            List<Cell> result = new List<Cell>();
            foreach (Cell cell in table.GetChildNodes(NodeType.Cell, true))
            {
                string Compare = cell.Range.Text;
                if (isContains)
                {
                    if (Compare.Contains(key))
                    {
                        result.Add(cell);
                    }
                }
                else
                {
                    if (Compare.Equals($"{key}\a"))
                    {
                        result.Add(cell);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 垂直合併欄位
        /// </summary>
        /// <param name="cellList">欄位</param>
        /// <param name="newValue">要寫入的新值</param>
        protected void VerticalMergeCells(List<Cell> cellList, string newValue = "")
        {
            foreach (Cell cell in cellList ?? new List<Cell>())
            {
                bool isFirst = cell.Equals(cellList.First());
                if (!string.IsNullOrEmpty(newValue) && isFirst)
                {
                    wordSetService.ReplaceText(cell, cell.Range.Text.Replace("\a", string.Empty), newValue);
                }

                cell.CellFormat.VerticalMerge = isFirst ? CellMerge.First : CellMerge.Previous;
            }
        }

        /// <summary>
        /// 水平合併欄位
        /// </summary>
        /// <param name="cellList">欄位</param>
        /// <param name="newValue">要寫入的新值</param>
        protected void HorizontalMergeCells(List<Cell> cellList, string newValue = "")
        {
            foreach (Cell cell in cellList ?? new List<Cell>())
            {
                bool isFirst = cell.Equals(cellList.First());
                if (!string.IsNullOrEmpty(newValue) && isFirst)
                {
                    wordSetService.ReplaceText(cell, cell.Range.Text.Replace("\a", string.Empty), newValue);
                }

                cell.CellFormat.HorizontalMerge = isFirst ? CellMerge.First : CellMerge.Previous;
            }
        }

        /// <summary>
        /// 新增對角線分隔cell
        /// </summary>
        /// <param name="titleLeft">文件產生器</param>
        /// <param name="titleRight">內容</param>
        /// <param name="mode">對角線方向</param>
        /// <param name="isBold">粗體</param>
        protected virtual Cell InsertThDig(string titleLeft, string titleRight, BorderType mode = BorderType.DiagonalDown, bool isBold = false)
        {
            Run runL = new Run(Doc, titleLeft);
            runL.Font.Bold = isBold;
            runL.Font.Size = Builder.Font.Size == 0 ? defaultFontSize : Builder.Font.Size;
            runL.Font.Name = defaultFontName;
            runL.Font.NameFarEast = defaultFontNameFarEast;
            runL.Font.NameOther = defaultFontNameFarEast;

            Run runR = new Run(Doc, titleRight);
            runR.Font.Bold = isBold;
            runR.Font.Size = Builder.Font.Size == 0 ? defaultFontSize : Builder.Font.Size;
            runR.Font.Name = defaultFontName;
            runR.Font.NameFarEast = defaultFontNameFarEast;
            runR.Font.NameOther = defaultFontNameFarEast;

            Cell cell = Builder.InsertCell();
            cell.CellFormat.LeftPadding = 2;
            cell.CellFormat.RightPadding = 2;

            Border diagonalBorder = cell.CellFormat.Borders[mode];
            diagonalBorder.Color = Color.Black;
            diagonalBorder.LineStyle = LineStyle.Single;
            diagonalBorder.LineWidth = 1;

            Paragraph P = new Paragraph(Doc);
            P.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            P.AppendChild(runL);

            cell.AppendChild(P);
            cell.FirstParagraph.ParagraphFormat.Alignment = ParagraphAlignment.Right;
            cell.FirstParagraph.AppendChild(runR);
            return cell;
        }

        /// <summary>
        /// 新增 title 格式的 cell
        /// </summary>
        /// <param name="title">內容</param>
        /// <param name="backGroundColor">可變換背景顏色</param>
        /// <param name="alignment">水平對齊方式</param>
        /// <param name="isBold">粗體</param>
        /// <param name="fontColor">文字顏色</param>
        /// <returns></returns>
        private Cell InsertTh(string title, Color backGroundColor = new Color(), ParagraphAlignment alignment = ParagraphAlignment.Center, bool isBold = false, Color fontColor = new Color())
        {
            if (backGroundColor.IsEmpty)
            {
                backGroundColor = Color.White;
            }

            Cell cell = Builder.InsertCell();
            cell.CellFormat.Borders[BorderType.DiagonalDown].LineStyle = LineStyle.None;
            cell.CellFormat.Shading.BackgroundPatternColor = backGroundColor;
            cell.CellFormat.Shading.ForegroundPatternColor = backGroundColor;
            cell.CellFormat.VerticalAlignment = CellVerticalAlignment.Center;
            cell.CellFormat.Orientation = TextOrientation.Horizontal;
            cell.CellFormat.HorizontalMerge = CellMerge.None;
            cell.CellFormat.VerticalMerge = CellMerge.None;
            cell.CellFormat.LeftPadding = 5;
            cell.CellFormat.RightPadding = 5;
            cell.CellFormat.TopPadding = 5;
            cell.CellFormat.BottomPadding = 5;
            Builder.ParagraphFormat.Alignment = alignment;
            Builder.Font.Size = Builder.Font.Size == 0 ? defaultFontSize : Builder.Font.Size;
            Builder.Font.Bold = isBold;
            Builder.Font.Color = fontColor.IsEmpty ? Color.Black : fontColor;
            Builder.Write(title);
            return cell;
        }

        /// <summary>
        /// 設定 title 格式的 cell
        /// </summary>
        /// <param name="title">內容</param>
        /// <param name="backGroundColor">可變換背景顏色</param>
        /// <param name="alignment">水平對齊方式</param>
        /// <param name="isBold">粗體</param>
        /// <param name="hMergeCount">水平合併數量</param>
        /// <param name="vMerge">重值合併</param>
        /// <param name="fontColor">文字顏色</param>
        protected void SetThColumn(string title, Color backGroundColor = new Color(), ParagraphAlignment alignment = ParagraphAlignment.Center, bool isBold = false, int? hMergeCount = null, CellMerge vMerge = CellMerge.None, Color fontColor = new Color())
        {
            Cell cell = InsertTh(title, backGroundColor: backGroundColor, alignment: alignment, isBold: isBold, fontColor: fontColor);

            if (vMerge != CellMerge.None)
            {
                cell.CellFormat.VerticalMerge = vMerge;
            }

            if (hMergeCount.HasValue)
            {
                cell.CellFormat.HorizontalMerge = CellMerge.First;
                for (int i = 0; i < hMergeCount.Value - 1; i++)
                {
                    InsertTh(string.Empty).CellFormat.HorizontalMerge = CellMerge.Previous;
                }
            }
        }

        /// <summary>
        /// 新增 content 格式的 cell
        /// </summary>
        /// <param name="content">內容</param>
        /// <param name="backGroundColor">可變換背景顏色</param>
        /// <param name="isBold">粗體?</param>
        /// <param name="ZeroEQspace">以空白取代0?</param>
        /// <param name="alignment">指定水平對齊方式</param>
        /// <param name="format">指定字型</param>
        /// <param name="fontColor">文字顏色</param>
        /// <returns></returns>
        private Cell InsertTd<T>(T content, Color backGroundColor = new Color(), bool isBold = false, bool ZeroEQspace = false, int? alignment = null, string format = null, Color fontColor = new Color())
        {
            if (backGroundColor.IsEmpty)
            {
                backGroundColor = Color.White;
            }

            Cell cell = Builder.InsertCell();
            cell.CellFormat.Borders[BorderType.DiagonalDown].LineStyle = LineStyle.None;
            cell.CellFormat.Shading.BackgroundPatternColor = backGroundColor;
            cell.CellFormat.Shading.ForegroundPatternColor = backGroundColor;
            cell.CellFormat.VerticalAlignment = CellVerticalAlignment.Center;
            cell.CellFormat.Orientation = TextOrientation.Horizontal;
            cell.CellFormat.VerticalMerge = CellMerge.None;
            cell.CellFormat.HorizontalMerge = CellMerge.None;
            cell.CellFormat.LeftPadding = 5;
            cell.CellFormat.RightPadding = 5;
            cell.CellFormat.TopPadding = 5;
            cell.CellFormat.BottomPadding = 5;

            Builder.Font.Size = Builder.Font.Size == 0 ? defaultFontSize : Builder.Font.Size;
            Builder.Font.Bold = isBold;
            Builder.Font.Color = fontColor.IsEmpty ? Color.Black : fontColor;
            Builder.ParagraphFormat.FirstLineIndent = 0;
            Builder.ParagraphFormat.LeftIndent = 0;

            if (typeof(T) == typeof(string))
            {
                //文字型態
                Builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            }
            else
            {
                //數字型態....應該也沒其它了吧....
                Builder.ParagraphFormat.Alignment = ParagraphAlignment.Right;
            }

            if (alignment.HasValue)
            {
                Builder.ParagraphFormat.Alignment = (ParagraphAlignment)alignment;
            }

            Type[] TypeInt = new Type[] { typeof(int), typeof(int?) };
            Type[] TypeLong = new Type[] { typeof(long), typeof(long?) };
            Type[] TypeDouble = new Type[] { typeof(double), typeof(double?), typeof(float), typeof(float?), typeof(decimal), typeof(decimal?) };

            if (content == null)
            {
                Builder.Write(string.Empty);
            }
            else if (typeof(T) == typeof(String))
            {
                Builder.Write(Convert.ToString(content) ?? string.Empty);
            }
            else if (TypeInt.Contains(typeof(T)) || TypeInt.Contains(content.GetType()))
            {
                if (ZeroEQspace && Convert.ToInt32(content) == 0)
                {
                    Builder.Write(string.Empty);
                }
                else
                {
                    Builder.Write(Convert.ToInt32(content).ToString("N0"));
                }
            }
            else if (TypeLong.Contains(typeof(T)) || TypeLong.Contains(content.GetType()))
            {
                Builder.Write(Convert.ToInt64(content).ToString("N0"));
            }
            else if (TypeDouble.Contains(typeof(T)) || TypeDouble.Contains(content.GetType()))
            {
                if (ZeroEQspace && Convert.ToDouble(content) == 0)
                {
                    Builder.Write(string.Empty);
                }
                else if (!string.IsNullOrEmpty(format))
                {
                    Builder.Write(Convert.ToDouble(content).ToString(format));
                }
                else
                {
                    // 只要是小數點都要顯示"*.00"
                    Builder.Write(Convert.ToDouble(content).ToString("N2"));
                }
            }
            else
            {
                Builder.Write(content.ToString());
            }

            return cell;
        }

        /// <summary>
        /// 設定 content 格式的 cell
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="backGroundColor">指定背景顏色</param>
        /// <param name="isBold">粗體</param>
        /// <param name="alignment">文字對齊值</param>
        /// <param name="format">指定字型</param>
        /// <param name="hMergeCnt">水平合併數量</param>
        /// <param name="vMerge">垂直合併</param>
        /// <param name="textOrientation">文字方向</param>
        /// <param name="fontColor">文字顏色</param>
        protected void SetTdColumn<T>(T value, Color backGroundColor = new Color(), bool isBold = false, AlignmentEnum alignment = AlignmentEnum.None, string format = null, int? hMergeCnt = null, CellMerge vMerge = CellMerge.None, TextOrientation textOrientation = TextOrientation.Horizontal, Color fontColor = new Color())
        {
            Cell cell = InsertTd(value,
                backGroundColor: backGroundColor,
                isBold: isBold,
                alignment: alignment == AlignmentEnum.None ? (int?)null : (int)alignment,
                format: format,
                fontColor: fontColor
            );

            if (textOrientation != TextOrientation.Horizontal)
            {
                cell.CellFormat.Orientation = textOrientation;
            }

            if (vMerge != CellMerge.None)
            {
                cell.CellFormat.VerticalMerge = vMerge;
            }

            if (hMergeCnt.HasValue)
            {
                cell.CellFormat.HorizontalMerge = CellMerge.First;
                for (int i = 0; i < hMergeCnt.Value - 1; i++)
                {
                    InsertTd(string.Empty).CellFormat.HorizontalMerge = CellMerge.Previous;
                }
            }
        }

        /// <summary>
        /// 設定空白的行
        /// </summary>
        /// <param name="columnCount">欄數</param>
        /// <param name="backGroundColor">指定背景顏色</param>
        /// <param name="alignment">文字對齊值</param>
        /// <param name="hMergeCnt">水平合併數量</param>
        /// <param name="vMerge">重值合併</param>
        protected void SetEmptyRow(int columnCount, Color backGroundColor = new Color(), AlignmentEnum alignment = AlignmentEnum.Center, int? hMergeCnt = null, CellMerge vMerge = CellMerge.None)
        {
            for (var i = 0; i < columnCount; i++)
            {
                SetTdColumn(string.Empty, backGroundColor: backGroundColor, alignment: alignment, hMergeCnt: hMergeCnt, vMerge: vMerge);
            }
        }

        /// <summary>
        /// 新增圖檔
        /// </summary>
        /// <param name="model"></param>
        protected void InsertChart(WordChartModel model)
        {
            // 建立圖檔 設定寬高
            Chart chart = Builder.InsertChart(model.ChartType, model.Width, model.Height).Chart;

            // 標題
            chart.Title.Show = !string.IsNullOrEmpty(model.TitleText);
            chart.Title.Text = model.TitleText;

            // 圖例位置
            chart.Legend.Position = model.LegendPosition;
            chart.Legend.Overlay = model.LegendOverlay;

            // X軸
            if (chart.AxisX != null && model.IsAxisXHidden)
            {
                chart.AxisX.Hidden = model.IsAxisXHidden;
            }

            // Y軸
            if (chart.AxisY != null && model.IsAxisYHidden)
            {
                chart.AxisY.Hidden = model.IsAxisYHidden;
            }

            // 處裡資料來源
            chart.Series.Clear();
            foreach (WordSeriesCollModel seriesColl in model.SeriesColls)
            {
                ChartSeries series = chart.Series.Add(seriesColl.SeriesName, model.Categories, seriesColl.Values);
                series.HasDataLabels = seriesColl.HasDataLabels;

                series.DataLabels.ShowValue = seriesColl.ShowValue;
                series.DataLabels.ShowLeaderLines = seriesColl.ShowLeaderLines;
                series.DataLabels.ShowSeriesName = seriesColl.ShowSeriesName;
                series.DataLabels.ShowLegendKey = seriesColl.ShowLegendKey;
                series.DataLabels.ShowCategoryName = seriesColl.ShowCategoryName;
                series.DataLabels.ShowPercentage = seriesColl.ShowPercentage;
                series.DataLabels.Separator = seriesColl.Separator;
                series.DataLabels.NumberFormat.FormatCode = seriesColl.Format;
            }
        }

        /// <summary>
        /// 設定重複標題
        /// </summary>
        /// <param name="rowIndexs">要重複的索引清單</param>
        protected void SetRepeatHeader(List<int> rowIndexs)
        {
            (rowIndexs ?? new List<int>()).ForEach(x =>
            {
                Table.Rows[x].RowFormat.HeadingFormat = true;
            });
        }
    }
}
