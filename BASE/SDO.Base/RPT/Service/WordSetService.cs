using System;
using System.Collections.Generic;
using System.Linq;
using Aspose.Words;
using Aspose.Words.Replacing;
using Aspose.Words.Saving;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using Aspose.Words.Tables;
using SDO.Models;
using SDO.ReportBuilder.Models;
using System.Drawing;

namespace SDO.Services
{
    public class WordSetService : Service, IWordSetService
    {
        /// <summary>
        /// 已建立之aspose word 物件
        /// </summary>
        public Document doc { get; set; }

        /// <summary>
        /// 格式化欄位
        /// </summary>
        public List<string> FormatColumns { get; set; }

        /// <summary>
        /// 輸出mimeType
        /// </summary>
        public string mimeType { get { return _mimeType; } }

        private string _mimeType;

        /// <summary>
        /// 表格內容文字大小
        /// </summary>
        private double DataFontSize;

        public MemoryStream GenWord(object basicData, IEnumerable<object> listData, string tempFilePath, int startRow, SaveFormat format, int tableindex = 0)
        {
            MemoryStream stream = new MemoryStream();
            GenWord(stream, basicData, listData, tempFilePath, startRow, format, tableindex);
            return stream;
        }

        public void GenWord(object basicData, IEnumerable<object> listData, string tableName)
        {
            Table table = (Table)FindTable(doc, tableName, true); //依傳入Index取得Table
            GenLoopTableByList(table, listData);//依填入資料完成Table
            ReplaceByObject(doc, basicData);//填入Table以外欄位
        }

        /// <summary>
        /// 自動填入資料及支援填入多TABLE
        /// </summary>
        /// <param name="basicData"></param>
        /// <param name="tableDatas"></param>
        public void GenWord(object basicData, IEnumerable<ITableData> tableDatas)
        {
            if (tableDatas != null)
            {
                foreach (ITableData tableData in tableDatas)
                    GenLoopTableByList(doc, tableData);//依傳入ITableData完成各Table
            }
            var basicDataDict = basicData as IDictionary<string, object>;//判斷傳入是否為Dictionary
            if (basicDataDict == null)
                ReplaceByObject(doc, basicData);//依Object填入Table以外欄位
            else
                ReplaceByDictionary(doc, basicDataDict);//依Dictionary填入Table以外欄位
        }

        /// <summary>
        /// 清除未取代的tag
        /// </summary>
        /// <param name="node"></param>
        public void CleanTag(Node node)
        {
            Regex searchTag = new Regex(@"#\w+#");
            node.Range.Replace(searchTag, "", new FindReplaceOptions() { MatchCase = false, FindWholeWordsOnly = false });
        }

        /// <summary>
        /// 自動填入資料及為單一TABLE填入資料
        /// </summary>
        /// <param name="outStream"></param>
        /// <param name="basicData"></param>
        /// <param name="listData"></param>
        /// <param name="tempFilePath"></param>
        /// <param name="startRow"></param>
        /// <param name="format"></param>
        /// <param name="tableindex"></param>
        public void GenWord(Stream outStream, object basicData, IEnumerable<object> listData, string tempFilePath, int startRow, SaveFormat format, int tableindex = 0)
        {
            doc = doc == null ? new Document(tempFilePath) : doc;//取得範本
            Table table = (Table)doc.GetChild(NodeType.Table, tableindex, true); //依傳入Index取得Table
            GenLoopTableByList(table, listData, startRow);//依填入資料完成Table

            ReplaceByObject(doc, basicData);//填入Table以外欄位

            doc.Save(outStream, format);//寫入Stream
        }

        /// <summary>
        /// 自動填入資料及支援填入多TABLE
        /// </summary>
        /// <param name="outStream"></param>
        /// <param name="basicData"></param>
        /// <param name="tableDatas"></param>
        /// <param name="tempFilePath"></param>
        /// <param name="format"></param>
        public void GenWord(Stream outStream, object basicData, IEnumerable<ITableData> tableDatas, string tempFilePath, SaveFormat format)
        {
            doc = doc == null ? new Document(tempFilePath) : doc;//取得範本

            foreach (ITableData tableData in tableDatas)
                GenLoopTableByList(doc, tableData);//依傳入ITableData完成各Table

            ReplaceByObject(doc, basicData);//填入Table以外欄位

            doc.Save(outStream, format);//寫入Stream
        }

        /// <summary>
        /// 依傳入ITableData完成TABLE
        /// </summary>
        /// <param name="node"></param>
        /// <param name="tableData"></param>
        public void GenLoopTableByList(CompositeNode node, ITableData tableData)
        {
            //依TABLE_NAME取的TABLE
            //TABLE_NAME優先於TABLE_INDEX

            Table table = string.IsNullOrEmpty(tableData.TABLE_NAME) ?
                (Table)node.GetChild(NodeType.Table, tableData.TABLE_INDEX, true) : FindTable(node, tableData.TABLE_NAME, true);
            //依資料生成TABLE
            GenLoopTableByList(table, tableData.LIST_DATA, tableData.CLONE_ROW_INDEX);
            //填入COMMOM_DATA
            ReplaceByObject(table, tableData.COMMOM_DATA);
        }

        /// <summary>
        /// 依傳入資料垂直GenRow
        /// </summary>
        /// <param name="table">欲填入資料之Table</param>
        /// <param name="datas">欲填入資料</param>
        /// <param name="cloneRowIndex">範例Row所在Index</param>
        public void GenLoopTableByList(Table table, IEnumerable<object> datas, int cloneRowIndex = 1)
        {
            //傳入資料是否為字典
            var dictData = datas.FirstOrDefault() as IDictionary<string, object>;
            bool isDictData = dictData != null;
            //找第一個參數欄位名稱，用來尋找 Target Table
            string FirstColumnName = "";

            if (!isDictData) // 匿名物件取得第一個參數欄位名稱
                FirstColumnName = datas.FirstOrDefault().GetType().GetProperties()[0].Name;
            else // 字典取得第一筆的Key
                FirstColumnName = dictData.FirstOrDefault().Key;
            //Loop Source(範本列)
            //以data參數名稱找範本列
            Row cloneRow = FindRow(table, FirstColumnName);
            Run firstRun = (Run)(cloneRow).GetChild(NodeType.Run, 0, true);
            DataFontSize = firstRun.Font.Size;
            //Loop區域末列Index
            //用於操作Loop區域之下的ROW
            int buttomIndex = 0;

            foreach (object data in datas)
            {
                Row newRow = (Row)cloneRow.Clone(true);//複製範本列
                var dataDict = data as IDictionary<string, object>;//判斷傳入是否為Dictionary
                if (dataDict==null)
                    ReplaceByObject(newRow, data);//依Object填入Table以外欄位
                else
                    ReplaceByDictionary(newRow, dataDict);

                // 設定儲存格背景樣式
                SetCellBgColor(newRow.Cells, data);

                //ReplaceByObject(newRow, data);//替換標籤
                table.InsertBefore(newRow, cloneRow);//Insert至範本列前
                buttomIndex = table.Rows.IndexOf(newRow);
            }

            cloneRow.Remove();//刪除範本列

            //顯示無資料ROW 需另設定樣式(字形、字色、大小等)
            //依各專案需求開啟或關閉
            bool showNoDataRow = false;
            if (showNoDataRow && //開關
                (datas == null || !datas.Any()) //判斷是否無資料
                )
            {
                Row emptyRow = new Row(table.Document); //新增ROW
                Cell emptyCell = new Cell(emptyRow.Document); //新增CELL
                emptyCell.AppendChild(new Paragraph(emptyCell.Document)); //新增段落
                emptyCell.FirstParagraph.Runs.Add(new Run(emptyCell.Document, "無資料")); //段落中新增文字
            }

            //計算項數 #LIST_COUNT# => datas.Count()
            //依各專案需求開啟或關閉
            ReplaceText(table, "#LIST_COUNT#", datas.Count().ToString());
        }

        /// <summary>
        /// 設定儲存格背景填滿
        /// </summary>
        /// <param name="cells"></param>
        /// <param name="data"></param>
        private void SetCellBgColor(CellCollection cells , object data) 
        {
            PropertyInfo[] props = data.GetType().GetProperties();
            Color bgColor = Color.Empty;
            foreach (Color color in CheckHaveBgColorProp(props, data))
            {
                if (color != Color.Empty)
                    bgColor = color;
            }
            if (bgColor != Color.Empty)
            {
                foreach (Cell cell in cells)
                {
                    cell.CellFormat.Shading.BackgroundPatternColor = bgColor;
                }
            }
        }

        /// <summary>
        /// 檢查物件屬性中是否有設定背景顏色屬性
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        private IEnumerable<Color> CheckHaveBgColorProp(PropertyInfo[] props, object data)
        {
            foreach(PropertyInfo prop in props)
            {
                if(prop.PropertyType == typeof(Color) && prop.Name == "BgColor")
                {
                    Color bgColor =  (Color)prop.GetValue(data);
                    yield return bgColor;
                    yield break;
                }
                yield return Color.Empty;
            }
        }

        /// <summary>
        /// 依物件屬性(Property)替換文字內容
        /// </summary>
        /// <param name="node">替換目標Node</param>
        /// <param name="data">替換資料</param>
        public void ReplaceByObject(Node node, object data)
        {
            if (data == null)
                return;
            PropertyInfo[] props = data.GetType().GetProperties();
            string[] arryTest = new string[] { "\r\n", "\n" };
            foreach (PropertyInfo prop in props)
            {
                string value = prop.GetValue(data)?.ToString();
                value = value ?? "";
                ReplaceText(node, $"#{prop.Name}#", prop.GetValue(data)?.ToString());
            }
        }

        /// <summary>
        /// 依Dictionary替換文字內容
        /// </summary>
        /// <param name="node">替換目標Node</param>
        /// <param name="keyValuePairs">替換資料</param>
        public void ReplaceByDictionary(Node node, IEnumerable<KeyValuePair<string, object>> keyValuePairs)
        {
            foreach (KeyValuePair<string, object> keyValuePair in keyValuePairs)
                ReplaceText(node, $"#{keyValuePair.Key}#", keyValuePair.Value!=null ? keyValuePair.Value.ToString():"");
        }

        /// <summary>
        /// 替換文字
        /// </summary>
        /// <param name="node">替換目標Node</param>
        /// <param name="oldValue">被替換value</param>
        /// <param name="value">替換value</param>
        public void ReplaceText(Node node, string oldValue, string value)
        {
            value ??= string.Empty; //避免value為null
            value = HtmlDecode(value); //Decode

            var optionArryTxt = new FindReplaceOptions();
            bool isformat = FormatColumns != null && FormatColumns.Any(x => x.Equals(oldValue));
            optionArryTxt.ReplacingCallback = new ReplaceTxt()
            {
                fontSize = DataFontSize,
                isAllAddStartSpace = false,
                isformat = isformat
            };
            node.Range.Replace(oldValue, value, optionArryTxt);
        }

        /// <summary>
        /// 存檔
        /// </summary>
        /// <param name="format">doc,docx,pdf,html</param>
        /// <returns></returns>
        public MemoryStream Save(string format = "docx")
        {
            //Builder.PageSetup.Orientation = _PageOrientation;
            var stream = new MemoryStream();
            SaveFormat saveFormat;
            switch (format.ToLower())
            {
                case "doc":
                    _mimeType = UCTableExport.MIME_ODT;
                    saveFormat = SaveFormat.Doc;
                    break;
                case "odt":
                    _mimeType = UCTableExport.MIME_ODT;
                    saveFormat = SaveFormat.Odt;
                    break;
                case "pdf":
                    _mimeType = UCTableExport.MIME_PDF;
                    saveFormat = SaveFormat.Pdf;
                    break;
                case "htm":
                case "html":
                    format = "html";
                    _mimeType = "text/html";
                    saveFormat = SaveFormat.Html;
                    HtmlSaveOptions options = new HtmlSaveOptions(SaveFormat.Html);
                    doc.Save(stream, options);
                    break;
                default:
                    _mimeType = UCTableExport.MIME_DOC;
                    saveFormat = SaveFormat.Docx;
                    break;
            }
            if (format != "html")
                doc.Save(stream, saveFormat);
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// 匯入Doc
        /// </summary>
        /// <param name="From">來源</param>
        /// <param name="To">匯入至</param>
        /// <param name="ResetPageNum">是否重設頁碼</param>
        public void ImportDoc(Document From, Document To, bool ResetPageNum = true)
        {
            NodeImporter importer = new NodeImporter(From, To.Document, ImportFormatMode.KeepSourceFormatting);
            int NodeIndex = 0;
            foreach (var Sec in From.GetChildNodes(NodeType.Section, true))
            {
                Section importNode = (Section)importer.ImportNode(Sec, true);
                if (NodeIndex == 0 && ResetPageNum)
                {
                    importNode.PageSetup.RestartPageNumbering = true;
                    importNode.PageSetup.PageStartingNumber = 1;
                }
                To.Document.AppendChild(importNode);
                NodeIndex++;
            }
        }

        /// <summary>
        /// 搜尋第一行第一列值包含#TABLE_{tableName}#的table
        /// 注意:會刪除第一列
        /// </summary>
        /// <param name="node"></param>
        /// <param name="tableName"></param>
        /// <param name="isDeep"></param>
        /// <returns></returns>
        public Table FindTable(CompositeNode node, string tableName, bool isDeep = true)
        {
            NodeCollection tables = node.GetChildNodes(NodeType.Table, isDeep);//取出全部Table
            Regex regex = new Regex($"#TABLE_{tableName}#", RegexOptions.IgnoreCase);//生成Regex
            Table matchTable = null;

            foreach (Node tableNode in tables)
            {
                Table table = (Table)tableNode;
                if (regex.IsMatch(table.FirstRow.FirstCell.GetText()))//判斷第一行第一列是否Match Regex
                {
                    matchTable = table;
                    break;
                }
            }

            //移除名稱列(第一列)
            matchTable?.RemoveChild(matchTable?.FirstRow);

            return matchTable;
        }

        /// <summary>
        /// 找某張表格的某行
        /// </summary>
        /// <param name="T">表格</param>
        /// <param name="key">搜尋字串</param>
        /// <returns></returns>
        public Row FindRow(Table T, string key)
        {
            foreach (Aspose.Words.Tables.Row R in T.GetChildNodes(NodeType.Row, true))
            {
                string Compare = R.Range.Text;
                if (Compare.Contains(key))
                {
                    return R;
                }
            }
            return null;
        }

        /// <summary>
        /// 進行 資料換行
        /// </summary>
        private class ReplaceTxt : IReplacingCallback
        {
            public bool isformat { get; set; }
            public CellMerge Hmerge { get; set; }
            public CellMerge Vmerge { get; set; }
            public double fontSize { get; set; }
            public bool isAllAddStartSpace { get; set; }
            ReplaceAction IReplacingCallback.Replacing(ReplacingArgs e)
            {
                // 取得node
                Node currentNode = e.MatchNode;
                // 移到node位置
                DocumentBuilder builder = new DocumentBuilder((Document)e.MatchNode.Document);
                builder.MoveTo(currentNode);
                if (isformat)
                    WriteInWithParagraph(ref builder, e.Replacement, isAllAddStartSpace, fontSize);
                else
                    e.Replacement = e.Replacement.Replace("\r\n", "\v");
                return ReplaceAction.Replace;
            }
        }

        /// <summary>
        /// 寫入文字(遇到特殊段落斷行)
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="content"></param>
        /// <param name="cellConfig"></param>
        /// <param name="isAllAddStartSpace">是否所有段落均要換行</param>
        public static void WriteInWithParagraph(ref DocumentBuilder builder, string content, bool isAllAddStartSpace = true, double fontSize = 12)
        {
            //未設定
            builder.Font.Size = fontSize;
            builder.Font.Bold = false;
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Justify;
            content = (content ?? "").Replace("\r\n", "\n");
            string[] stringSeparators = new string[] { "\n" };
            string[] lines = content.Split(stringSeparators, StringSplitOptions.None);
            string txt = "";
            int lineOrignDistance = Convert.ToInt32(fontSize);
            int tmpIndex = 0;
            //依照每行開頭判斷格式與縮排
            foreach (var text in lines)
            {
                Match match = null;
                builder.Font.Bold = false;
                txt = text;
                if (Regex.IsMatch(text, @"^[●]"))
                {
                    match = Regex.Match(text, "^[●]");
                    builder.ParagraphFormat.FirstLineIndent = -8;
                    builder.ParagraphFormat.LeftIndent = lineOrignDistance;
                }
                if (Regex.IsMatch(text, @"^[一|二|三|四|五|六|七|八|九|十]+[、]"))
                {
                    match = Regex.Match(text, "^[一|二|三|四|五|六|七|八|九|十]+[、]");
                    builder.ParagraphFormat.FirstLineIndent = -lineOrignDistance * (match.Length);
                    builder.ParagraphFormat.LeftIndent = lineOrignDistance * (match.Length);
                }
                else if (Regex.IsMatch(text, @"^[(|（][一|二|三|四|五|六|七|八|九|十]+[)|）]"))
                {
                    txt = txt.Replace("(", "（").Replace(")", "）");
                    match = Regex.Match(text, "^[(|（][一|二|三|四|五|六|七|八|九|十]+[)|）]");
                    builder.ParagraphFormat.FirstLineIndent = -lineOrignDistance * (match.Length);
                    builder.ParagraphFormat.LeftIndent = lineOrignDistance * (match.Length + 1);
                }
                else if (Regex.IsMatch(text, @"^[１|２|３|４|５|６|７|８|９|０|0-9]+[、]"))
                {
                    match = Regex.Match(text, "^[１|２|３|４|５|６|７|８|９|０|0-9]+[、]");
                    builder.ParagraphFormat.FirstLineIndent = -lineOrignDistance * (match.Length);
                    builder.ParagraphFormat.LeftIndent = lineOrignDistance * (match.Length + 2);
                }
                else if (Regex.IsMatch(text, @"^[(|（][１|２|３|４|５|６|７|８|９|０|0-9]+[)|）]"))
                {
                    txt = txt.Replace("(", "（").Replace(")", "）");
                    match = Regex.Match(text, "^[(|（][１|２|３|４|５|６|７|８|９|０|0-9]+[)|）]");
                    builder.ParagraphFormat.FirstLineIndent = -lineOrignDistance * (match.Length);
                    builder.ParagraphFormat.LeftIndent = lineOrignDistance * (match.Length + 3);
                }
                if (match == null && isAllAddStartSpace)
                    builder.ParagraphFormat.FirstLineIndent = lineOrignDistance * 2;
                txt = txt.Replace(" ", "");
                if (++tmpIndex == lines.Length)
                    builder.Write(txt);
                else
                    builder.Writeln(txt);
            }
        }
    }
}
