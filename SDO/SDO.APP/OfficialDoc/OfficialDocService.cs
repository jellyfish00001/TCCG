using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Aspose.BarCode.Generation;
using Aspose.Words;
using Aspose.Words.Drawing;
using Aspose.Words.Lists;
using Aspose.Words.Saving;
using Aspose.Words.Tables;
using HtmlAgilityPack;
using SDO.Models;
using Cell = Aspose.Words.Tables.Cell;
using Color = System.Drawing.Color;
using DashStyle = Aspose.Words.Drawing.DashStyle;
using Document = Aspose.Words.Document;
using HeaderFooter = Aspose.Words.HeaderFooter;
using HorizontalAlignment = Aspose.Words.Drawing.HorizontalAlignment;
using Image = System.Drawing.Image;
using Matrix = System.Drawing.Drawing2D.Matrix;
using PdfSaveOptions = Aspose.Words.Saving.PdfSaveOptions;
using Rectangle = System.Drawing.Rectangle;
using Row = Aspose.Words.Tables.Row;
using SaveFormat = Aspose.Words.SaveFormat;
using Table = Aspose.Words.Tables.Table;
using VerticalAlignment = Aspose.Words.Drawing.VerticalAlignment;
using SDO.Utils;

namespace SDO.Services
{
    public class OfficialDocService : Service, IOfficialDocService
    {
        public byte[] ExportHTMLToWord(string offDoc)
        {
            // 公文編輯器傳遞至後端需解碼
            string decodeOffDoc = HtmlDecode(offDoc);
            // 設定傳入Model相關參數
            SEAttribute attribute = new SEAttribute();
            attribute.SEContent = decodeOffDoc;

            // 公文編輯器HTML轉換成可匯出的HTML
            SEExporter exporter = new SEExporter();
            Document doc = exporter.ExportWord(attribute, true);
            MemoryStream docStream = new MemoryStream();
            doc.Save(docStream, SaveFormat.Docx);

            return docStream.ToArray();
        }

        public byte[] ExportHTMLToPDF(string offDoc)
        {
            // 公文編輯器傳遞至後端需解碼
            string decodeOffDoc = HtmlDecode(offDoc);
            // 設定傳入Model相關參數
            SEAttribute attribute = new SEAttribute();
            attribute.SEContent = decodeOffDoc;
            // 公文編輯器HTML轉換成可匯出的HTML
            SEExporter exporter = new SEExporter();
            Document doc = exporter.ExportPdf(attribute);

            var options = new PdfSaveOptions
            {
                OptimizeOutput = true,
                TextCompression = PdfTextCompression.Flate,
                ImageCompression = PdfImageCompression.Auto,
                JpegQuality = 10,
                SaveFormat = SaveFormat.Pdf,
                ColorMode = Aspose.Words.Saving.ColorMode.Normal
            };
            MemoryStream docStream = new MemoryStream();
            doc.Save(docStream, options);
            return docStream.ToArray();
        }
    }

    /// <summary>
    /// 用來記錄RowSpan/ColSpan的起始Index與連續長度
    /// </summary>
    public class TableSpan
    {
        public TableSpan(int rowIndex, int rowSpanLength, int colIndex, int colSpanLength)
        {
            RowIndex = rowIndex;
            RowSpanLength = rowSpanLength;
            ColIndex = colIndex;
            ColSpanLength = colSpanLength;
        }

        /// <summary>
        /// RowIndex
        /// </summary>
        public int RowIndex { get; set; }
        /// <summary>
        /// Rowspan長度，如果rowspan為0則必須換算成從自己到最後的長度
        /// </summary>
        public int RowSpanLength { get; set; }
        /// <summary>
        /// ColIndex
        /// </summary>
        public int ColIndex { get; set; }
        /// <summary>
        /// Colspan長度，如果colspan為0則必須換算成從自己到最後的長度
        /// </summary>
        public int ColSpanLength { get; set; }

        public bool HasSlot(TableSpan nextSpan)
        {
            return nextSpan.ColIndex > ColIndex + ColSpanLength;
        }
    }

    public class TableData
    {
        public TableData()
        {
            CellWidths = new List<double>();
            SplitRows = new List<bool>();
            Type = TableType.Normal;
        }

        public TableData(List<double> cellWidths, List<bool> splitRows, TableType type = TableType.Normal)
        {
            CellWidths = cellWidths;
            SplitRows = splitRows;
            Type = type;
        }
        /// <summary>
        /// 存每個Cell的寬度
        /// </summary>
        public List<double> CellWidths { get; set; }

        /// <summary>
        /// 存每個TR是否分頁
        /// </summary>
        public List<bool> SplitRows { get; set; }

        public TableType Type { get; set; }
    }

    public enum TableType
    {
        Normal,
        ApprovedBox,
        Stamp
    }

    public class SEExporter
    {
        private Document _doc;
        private DocumentBuilder _builder;
        private HtmlDocument _htmlDoc;
        private Image _sealImage;
        private Image _sealImageRight;
        private string _sealImageRightName;
        private Image _sealImageLeft;
        private string _sealImageLeftName;
        private SkiaSharp.SKBitmap _gutterImage;
        private SkiaSharp.SKBitmap _watermarkImage;
        private XmlDocument _settingXml;
        private string _savePath;
        private string _stampCSS;
        private string _tableCSS;
        private string _OCSealCSS;
        private int _outKind;
        private bool _shrinkAddress;
        private string[] SkipCharList => new[] { "\r", "\n", "\r\n", "\t" };

        private Dictionary<string, HtmlNode> _listHtmlData;
        private int _listCount;
        private SaveFormat saveFormat;
        private List<TableData> _tableList;

        /// <summary>
        /// 預設中文字體，如果SE檔中的元素沒有額外指定字體則會自動套用此字體，預設為：標楷體
        /// </summary>
        public string DefaultFarEastFont { get; set; }
        /// <summary>
        /// 預設英文字體，如果SE檔中的元素沒有額外指定字體則會自動套用此字體，預設為：Times New Roman
        /// </summary>
        public string DefaultAsciiFont { get; set; }
        /// <summary>
        /// 預設字體大小，如果SE檔中的元素沒有額外指定字體大小則會自動套用此字體大小，預設為：16 pt
        /// Table元素會沒有指定大小，會自動套用預設
        /// </summary>
        public double DefaultFontSize { get; set; }

        /// <summary>
        /// 改為用Static Constructor來認證，整個應用程式的生命週期裡就只會執行一次
        /// </summary>
        static SEExporter()
        {
            //AsposeLicenseHelper.SetLicense();
            #region Aspose 認證
            Aspose.Cells.License licenseCells = new Aspose.Cells.License();
            Aspose.Words.License licenseWords = new Aspose.Words.License();
            Aspose.Pdf.License licensePdf = new Aspose.Pdf.License();
            licenseCells.SetLicense("License\\Aspose.Total.655.lic");
            licenseWords.SetLicense("License\\Aspose.Total.655.lic");
            licensePdf.SetLicense("License\\Aspose.Total.655.lic");

            #endregion
        }

        public SEExporter()
        {
            DefaultFarEastFont = "標楷體";
            DefaultAsciiFont = "Times New Roman";
            DefaultFontSize = 15D;
            _tableCSS = @"<style>
                            .table_new {
                                width: auto; 
                                table-layout: fixed;
                                font-size: 12pt;
                            }

                                .table_new, .table_new tr {
                                    height: 40px;
                                }

                                    .table_new, .table_new td {
                                        border-collapse: collapse;
                                        border: 1px solid #000000;
                                        padding:2px 5px 2px 5px;
                                    }
                                .TdAlignLeft {
                                    text-align: left;
                                }
          
                                .TdAlignRight {
                                    text-align: right;
                                }
          
                                .TdAlignCenter {
                                    text-align: center;
                                }
                                /*Add by Stan 20180206: 頭部表格樣式*/
                                .table_slim td {
                                    border-collapse: collapse;
                                    border: 0.5px solid #000000;
                                }
                                .table_slim, .table_slim tr { height:auto; }
                            </style>";
            _stampCSS = @"<style>
                            div[data-speed-id=seal-box]{
                                margin-top: 20px;
                                margin-left: 0px;
                                margin-right: 0px;
                                margin-bottom: 0px;
                                height: 20px;
                            }
                            td[data-speed-sealouid] {
                                width: 60px;
                            }
                            table[data-speed-id='seal-table'] {
                                border: 1px solid red !important;
                                color: red !important;
                                border-collapse: initial;
                            }
                            table[data-speed-id=seal-table] td{
                                padding:0px !important;
                                margin:0px !important;
                                border: 0px;
                                border-collapse: collapse;
                                border-spacing: 0px !important;
                                vertical-align: middle !important;
                            }
                            </style>";
        }

        /// <summary>
        /// 將SE檔轉換成DOC檔輸出
        /// </summary>
        /// <param name="attribute">Expoter的設定參數</param>
        /// <param name="office2007">是否輸出DOCX</param>
        public Document ExportWord(SEAttribute attribute, bool office2007 = false)
        {
            saveFormat = office2007 ? SaveFormat.Docx : SaveFormat.Doc;
            Load(attribute);
            Process();
            // 調整為回傳 Document 至呼叫專案，不在共用直接儲存
            //var fileName = office2007 ? $"{attribute.SaveFileName}.docx" : $"{attribute.SaveFileName}.doc";
            //_doc.Save(@Path.Combine(attribute.SavePath, fileName), saveFormat);

            // 20190904 改為如果有圖片，則該圖片的段落不設定行高
            foreach (Shape shape in _doc.GetChildNodes(NodeType.Shape, true).Select(data => (Shape)data))
            {
                if (shape.ParentNode.NodeType == NodeType.Paragraph)
                {
                    ((Paragraph)shape.ParentNode).ParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
                    ((Paragraph)shape.ParentNode).ParagraphFormat.LineSpacing = 0;
                }
            }
            return _doc;
        }
        /// <summary>
        /// 將SE檔轉換成ODT檔(OpenOffice)輸出，不支援騎縫章的功能
        /// </summary>
        /// <param name="attribute">Expoter的設定參數</param>
        public void ExportOdt(SEAttribute attribute)
        {
            saveFormat = SaveFormat.Odt;
            Load(attribute);
            //ODT不支援騎縫章輸出，所以設定圖檔為Null，不然會印出亂碼
            _sealImage = null;
            Process();
            _doc.Save(@Path.Combine(attribute.SavePath, attribute.SaveFileName + ".odt"), saveFormat);
        }
        /// <summary>
        /// 將SE檔轉換成RTF檔輸出，檔案會很大
        /// </summary>
        /// <param name="attribute">Expoter的設定參數</param>
        public void ExportRtf(SEAttribute attribute)
        {
            saveFormat = SaveFormat.Rtf;
            Load(attribute);
            Process();
            _doc.Save(@Path.Combine(attribute.SavePath, attribute.SaveFileName + ".rtf"), saveFormat);
        }
        /// <summary>
        /// 將SE檔轉換成TIFF檔輸出
        /// </summary>
        /// <param name="attribute">Expoter的設定參數</param>
        public void ExportTiff(SEAttribute attribute)
        {
            saveFormat = SaveFormat.Tiff;
            Load(attribute);
            Process();
            // 調整成高品質TIF輸出，不然解析度很差
            var options = new ImageSaveOptions(saveFormat)
            {
                UseAntiAliasing = true,
                UseHighQualityRendering = true,
                TiffCompression = TiffCompression.Lzw,
                Resolution = 204
            };
            _doc.Save(@Path.Combine(attribute.SavePath, attribute.SaveFileName + ".tiff"), options);
        }
        /// <summary>
        /// 將SE檔轉換成PDF檔輸出
        /// </summary>
        /// <param name="attribute">Expoter的設定參數</param>
        public Document ExportPdf(SEAttribute attribute)
        {
            //saveFormat = SaveFormat.Pdf;
            Load(attribute);
            Process();
            //var options = new PdfSaveOptions
            //{
            //    OptimizeOutput = true,
            //    TextCompression = PdfTextCompression.Flate,
            //    ImageCompression = PdfImageCompression.Auto,
            //    JpegQuality = 10,
            //    SaveFormat = saveFormat,
            //    ColorMode = ColorMode.Normal
            //};
            //_doc.Save(@Path.Combine(attribute.SavePath, attribute.SaveFileName + ".pdf"), options);
            return _doc;
        }
        /// <summary>
        /// 將SE檔轉換成TXT檔輸出 (不含頁碼、條碼、裝訂線、騎縫章)
        /// </summary>
        /// <param name="attribute">Expoter的設定參數</param>
        public void ExportTxt(SEAttribute attribute)
        {
            saveFormat = SaveFormat.Text;
            Load(attribute);
            Process(true);
            _doc.Save(@Path.Combine(attribute.SavePath, attribute.SaveFileName + ".txt"), saveFormat);
        }
        /// <summary>
        /// 將SE檔轉換成HTML檔輸出 (不含頁碼)
        /// </summary>
        /// <param name="attribute">Expoter的設定參數</param>
        public void ExportHtml(SEAttribute attribute)
        {
            saveFormat = SaveFormat.Html;
            Load(attribute);
            Process(true);
            _doc.Save(@Path.Combine(attribute.SavePath, attribute.SaveFileName + ".html"), saveFormat);
        }

        /// <summary>
        /// 輸出追蹤修訂+簽稿會核單 (PDF)
        /// </summary>
        /// <param name="attribute">Expoter的設定參數</param>
        public void ExportTrackChangsWithApproval(SEAttribute attribute)
        {
            saveFormat = SaveFormat.Pdf;
            var fileName = @Path.Combine(attribute.SavePath, $"{attribute.SaveFileName}APPROVAL.pdf");
            Load(attribute);
            //不需要騎縫章
            _sealImage = null;
            //公文輸出
            Process();
            //輸出追蹤修訂
            _doc.Save(fileName, saveFormat);
            var pdfDocument = new Aspose.Pdf.Document(fileName);
            File.Delete(fileName);

            //產生簽稿會核單
            var approvalDocs = GenerateApprovalDocs(attribute.ApprovalContent);
            //附加
            var appCount = 0;
            foreach (var approvalDoc in approvalDocs)
            {
                fileName = @Path.Combine(attribute.SavePath, $"{attribute.SaveFileName}APPROVAL{++appCount}.pdf");
                approvalDoc.FirstSection.PageSetup.SectionStart = SectionStart.NewPage;
                _doc.AppendDocument(approvalDoc, ImportFormatMode.KeepDifferentStyles);
                _doc.Sections[appCount].HeadersFooters.LinkToPrevious(false);
                //輸出每頁簽稿會核單
                approvalDoc.Save(fileName, saveFormat);
                pdfDocument.Pages.Add(new Aspose.Pdf.Document(fileName).Pages);
                File.Delete(fileName);
            }
            //輸出總檔
            pdfDocument.Save(@Path.Combine(attribute.SavePath, attribute.SaveFileName + ".pdf"));
        }

        /// <summary>
        /// 輸出本文PDF+簽稿會核單 (PDF)
        /// </summary>
        /// <param name="attribute">Expoter的設定參數</param>
        /// <param name="hasDocumentFile">是否有本文檔(TIFF: SEAttribute要傳入TIFFContent, PDF: SEAttribute要傳入PdfContent)</param>
        public void ExportPdfWithApproval(SEAttribute attribute, bool hasDocumentFile)
        {
            saveFormat = SaveFormat.Pdf;
            var pdfDocument = new Aspose.Pdf.Document();
            if (hasDocumentFile && attribute.TIFFContent != null)
            {
                //紙本:將TIFF每頁轉成PDF Page，並插到pdfDocument中
                var stream = new MemoryStream(attribute.TIFFContent);

                using (var tiff = Image.FromStream(stream))
                {
                    var pages = tiff.GetFrameCount(FrameDimension.Page);
                    for (var i = 0; i < pages; i++)
                    {
                        tiff.SelectActiveFrame(FrameDimension.Page, i);
                        stream = new MemoryStream();
                        tiff.Save(stream, ImageFormat.Tiff);
                        var page = pdfDocument.Pages.Add();
                        var background = new Aspose.Pdf.BackgroundArtifact { BackgroundImage = stream };
                        page.Artifacts.Add(background);
                    }
                    stream.Close();
                }
            }
            else if (hasDocumentFile && attribute.PdfContent != null)
            {
                //電子:已有本文檔，直接加入即可
                var stream = new MemoryStream(attribute.PdfContent);
                pdfDocument.Pages.Add(new Aspose.Pdf.Document(stream).Pages);
            }
            else
            {
                //電子或稿:將SEContent轉成PDF，再將PDF加到pdfDocument中
                Load(attribute);
                Process();
                var stream = new MemoryStream();
                _doc.Save(stream, saveFormat);
                pdfDocument.Pages.Add(new Aspose.Pdf.Document(stream).Pages);
            }

            //不需要騎縫章
            _sealImage = null;
            // Add by Stan 20171226: 自動從WholeApprovalContent切出每一頁，前端不用再轉成array傳入
            var approvalContent = new List<string>();
            if (!string.IsNullOrEmpty(attribute.WholeApprovalContent))
            {
                var htmlDoc = new HtmlDocument { OptionFixNestedTags = true };
                htmlDoc.LoadHtml(attribute.WholeApprovalContent);
                approvalContent.AddRange(htmlDoc.DocumentNode.ChildNodes.Select(node => node.OuterHtml));
                //htmlDoc = null;
            }
            else if (attribute.ApprovalContent != null)
            {
                //相容舊版
                approvalContent.AddRange(attribute.ApprovalContent);
            }
            //產生簽稿會核單
            var approvalDocs = GenerateApprovalDocs(approvalContent.ToArray());
            //附加
            foreach (var approvalDoc in approvalDocs)
            {
                //輸出每頁簽稿會核單
                var approvalDocStream = new MemoryStream();
                approvalDoc.Save(approvalDocStream, saveFormat);
                pdfDocument.Pages.Add(new Aspose.Pdf.Document(approvalDocStream).Pages);
            }
            //輸出總檔
            pdfDocument.Save(@Path.Combine(attribute.SavePath, attribute.SaveFileName + ".pdf"));
        }

        public void ExportApproval(SEAttribute attribute)
        {
            IEnumerable<Document> approvalDocs = GenerateApprovalDocs(attribute.ApprovalContent);
            SaveApproval(attribute, approvalDocs);
        }

        public void ExportWholeApproval(SEAttribute attribute)
        {
            var approvalContent = new List<string>();
            if (!string.IsNullOrEmpty(attribute.WholeApprovalContent))
            {
                var htmlDoc = new HtmlDocument { OptionFixNestedTags = true };
                htmlDoc.LoadHtml(attribute.WholeApprovalContent);
                approvalContent.AddRange(htmlDoc.DocumentNode.ChildNodes.Select(node => node.OuterHtml));
                //htmlDoc = null;
            }
            IEnumerable<Document> approvalDocs = GenerateApprovalDocs(approvalContent.ToArray());
            SaveApproval(attribute, approvalDocs);
        }

        public void SaveApproval(SEAttribute attribute, IEnumerable<Document> approvalDocs)
        {
            saveFormat = SaveFormat.Pdf;
            var pdfDocument = new Aspose.Pdf.Document();
            //不需要騎縫章
            _sealImage = null;

            //附加
            var appCount = 0;
            _doc = new Document();

            //產生簽稿會核單
            foreach (var approvalDoc in approvalDocs)
            {
                var fileName = @Path.Combine(attribute.SavePath, $"{attribute.SaveFileName}APPROVAL{++appCount}.pdf");
                approvalDoc.FirstSection.PageSetup.SectionStart = SectionStart.NewPage;
                _doc.AppendDocument(approvalDoc, ImportFormatMode.KeepDifferentStyles);
                _doc.Sections[appCount].HeadersFooters.LinkToPrevious(false);
                //輸出每頁簽稿會核單
                approvalDoc.Save(fileName, saveFormat);
                pdfDocument.Pages.Add(new Aspose.Pdf.Document(fileName).Pages);
                File.Delete(fileName);
            }

            //輸出總檔
            pdfDocument.Save(@Path.Combine(attribute.SavePath, attribute.SaveFileName + ".pdf"));
        }

        /// <summary>
        /// 輸出PDF，內含本文(TIFF/PDF)+職章&決行意見(StampSE)
        /// </summary>
        /// <param name="attribute">Expoter的設定參數</param>
        /// <param name="isPaper">是否為紙本(是的話要SEAttribute要傳入TIFFStream)</param>
        public void ExportPdfWithStamp(SEAttribute attribute, bool isPaper)
        {
            saveFormat = SaveFormat.Pdf;
            var pdfDocument = new Aspose.Pdf.Document();
            if (isPaper)
            {
                //紙本:將TIFF每頁轉成PDF Page，並插到pdfDocument中
                var stream = new MemoryStream(attribute.TIFFContent);
                using (var tiff = Image.FromStream(stream))
                {
                    var pages = tiff.GetFrameCount(FrameDimension.Page);
                    for (var i = 0; i < pages; i++)
                    {
                        tiff.SelectActiveFrame(FrameDimension.Page, i);
                        stream = new MemoryStream();
                        tiff.Save(stream, ImageFormat.Tiff);
                        var page = pdfDocument.Pages.Add();
                        var background = new Aspose.Pdf.BackgroundArtifact { BackgroundImage = stream };
                        page.Artifacts.Add(background);
                    }
                    stream.Close();
                }
            }
            else
            {
                //電子:將DI->SE轉成PDF，再將PDF加到pdfDocument中
                Load(attribute);
                Process();
                var stream = new MemoryStream();
                _doc.Save(stream, saveFormat);
                pdfDocument.Pages.Add(new Aspose.Pdf.Document(stream).Pages);
            }
            if (!string.IsNullOrEmpty(attribute.StampSE))
            {
                //TODO:待確認
                attribute.SealImage = null;
                attribute.GutterImage = null;
                attribute.BarCodeXml = null;
                attribute.WatermarkImage = null;
                //輸出職章&決行意見
                Load(attribute, true);
                Process();

                var stampStream = new MemoryStream();
                _doc.Save(stampStream, saveFormat);
                pdfDocument.Pages.Add(new Aspose.Pdf.Document(stampStream).Pages);
            }
            //輸出總檔
            pdfDocument.Save(@Path.Combine(attribute.SavePath, attribute.SaveFileName + ".pdf"));
        }

        private void Load(SEAttribute attribute, bool stamp = false)
        {
            //Init
            _doc = new Document();
            _doc.CompatibilityOptions.DoNotExpandShiftReturn = true;
            //Add by Stan 20171102: 表格內容不超過頁面寬度
            _doc.CompatibilityOptions.GrowAutofit = false;
            _builder = new DocumentBuilder(_doc);
            _htmlDoc = new HtmlDocument { OptionFixNestedTags = true };
            _sealImageLeft = null;
            _sealImageRight = null;
            _savePath = attribute.SavePath;
            _listHtmlData = new Dictionary<string, HtmlNode>();
            _listCount = 0;
            //Load
            _htmlDoc.LoadHtml(GetClearHtmlForExporter(stamp ? attribute.StampSE : attribute.SEContent));
            _sealImage = attribute.SealImage;
            _gutterImage = attribute.GutterImage;
            _watermarkImage = attribute.WatermarkImage;
            if (!string.IsNullOrEmpty(attribute.StampCSS)) _stampCSS = attribute.StampCSS;
            //BarCode modify by simon (新增BarCodeXml property)
            if (attribute.BarCodeXml != null)
            {
                _settingXml = attribute.BarCodeXml;
            }
            else if (!string.IsNullOrEmpty(attribute.BarCodeSettingXml))
            {
                _settingXml = new XmlDocument();
                _settingXml.XmlResolver = null;
                _settingXml.LoadXml(attribute.BarCodeSettingXml);
            }
            else
            {
                _settingXml = new XmlDocument();
                _settingXml.XmlResolver = null;
            }
            //OCSeal
            if (attribute.OCSealCSS != string.Empty)
            {
                _OCSealCSS = attribute.OCSealCSS;
                _outKind = attribute.OutKind;
            }
            //ShrinkAddress
            _shrinkAddress = attribute.ShrinkAddress;
        }

        private IEnumerable<Document> GenerateApprovalDocs(string[] contents)
        {
            if (contents == null)
                return new List<Document>();
            var resultDocs = new List<Document>();
            //var count = 0;
            foreach (var content in contents)
            {
                var tempDoc = new Document();
                var tempBuilder = new DocumentBuilder(tempDoc);
                var tempHtmlDoc = new HtmlDocument { OptionFixNestedTags = true };
                tempHtmlDoc.LoadHtml(GetClearHtml(content));
                // 設定Document的中英文預設字體
                tempDoc.Styles.DefaultParagraphFormat.Style.Font.NameFarEast = DefaultFarEastFont;
                tempDoc.Styles.DefaultParagraphFormat.Style.Font.NameAscii = DefaultAsciiFont;
                tempDoc.Styles.DefaultParagraphFormat.Style.Font.Size = DefaultFontSize;
                tempDoc.Styles.DefaultParagraphFormat.AddSpaceBetweenFarEastAndAlpha = false;
                tempDoc.Styles.DefaultParagraphFormat.AddSpaceBetweenFarEastAndDigit = false;
                // Add by Stan 20171113: DefaultFont需要一併設定 PDF才能正確換行
                tempDoc.Styles.DefaultFont.NameFarEast = DefaultFarEastFont;
                tempDoc.Styles.DefaultFont.NameAscii = DefaultAsciiFont;
                // 從SE檔讀取頁面基本配置
                //var pageOrientation = tempHtmlDoc.DocumentNode.FirstChild.GetAttributeValue("class", "Portrait").Contains("Portrait") ? "Portrait" : "Landscape";
                var pageSize = tempHtmlDoc.DocumentNode.FirstChild.GetAttributeValue("data-speed-pagesize", "A4");
                //var pageHeaderSpacing = double.Parse(tempHtmlDoc.DocumentNode.FirstChild.GetAttributeValue("data-speed-headerspacing", "20"));
                //var pageFooterSpacing = double.Parse(tempHtmlDoc.DocumentNode.FirstChild.GetAttributeValue("data-speed-footerspacing", "20"));
                var pageLeftSpacing = double.Parse(tempHtmlDoc.DocumentNode.FirstChild.GetAttributeValue("data-speed-leftspacing", "25"));
                var pageRightSpacing = double.Parse(tempHtmlDoc.DocumentNode.FirstChild.GetAttributeValue("data-speed-rightspacing", "25"));
                //var pageGutterLineSpacing = double.Parse(tempHtmlDoc.DocumentNode.FirstChild.GetAttributeValue("data-speed-gutterlinespacing", "5"));
                //設定頁面配置方式 (Aspose單位為Point, SE單位為mm, 所以需要轉換)
                //tempBuilder.CurrentSection.PageSetup.Orientation = (Orientation)Enum.Parse(typeof(Orientation), pageOrientation);
                tempBuilder.CurrentSection.PageSetup.PaperSize = (PaperSize)Enum.Parse(typeof(PaperSize), pageSize);
                tempBuilder.CurrentSection.PageSetup.TopMargin = 0;//ConvertUtil.MillimeterToPoint(pageHeaderSpacing);
                                                                   //Modify by Stan 20171211: FooterMargin設為0，讓table有多點空間
                tempBuilder.CurrentSection.PageSetup.BottomMargin = 0;//ConvertUtil.MillimeterToPoint(pageFooterSpacing);
                tempBuilder.CurrentSection.PageSetup.LeftMargin = ConvertUtil.MillimeterToPoint(pageLeftSpacing);
                tempBuilder.CurrentSection.PageSetup.RightMargin = ConvertUtil.MillimeterToPoint(pageRightSpacing);

                var textBefore = "";
                var textAfter = "";
                //Modify by Stan 20180209: 簽稿會核單範本可能會沒有header或footer，加上防呆
                //輸出Header的FullName
                tempBuilder.MoveToHeaderFooter(HeaderFooterType.HeaderPrimary);
                var headerNode = tempHtmlDoc.DocumentNode.SelectSingleNode("//div[@data-speed-id='FullName']");
                if (headerNode != null)
                {
                    var headerStyle = GetStyles(headerNode, tempBuilder);
                    tempBuilder.ParagraphFormat.Alignment = (ParagraphAlignment)Enum.Parse(typeof(ParagraphAlignment), headerStyle["text-align"], true);
                    tempBuilder.Font.Size = GetFontSize(headerNode);
                    textBefore = headerNode.GetAttributeValue("data-speed-before", "");
                    textAfter = headerNode.GetAttributeValue("data-speed-after", "");
                    tempBuilder.InsertHtml($"{textBefore}{headerNode.InnerHtml}{textAfter}", true);
                }

                tempBuilder.MoveToHeaderFooter(HeaderFooterType.FooterPrimary);
                var footerNodes = tempHtmlDoc.DocumentNode.SelectSingleNode("//div[@class='PageFooter']")?.ChildNodes;
                if (footerNodes != null)
                {
                    foreach (var footerNode in footerNodes)
                    {
                        //如果Node為換行符號(\n)或註解(<!--123-->)則跳過 Add by Stan 20180226: 頁碼防呆
                        if (footerNode.OriginalName == "#text" || footerNode.OriginalName == "#comment" || footerNode.GetAttributeValue("class", "").Contains("noprint") || !footerNode.HasChildNodes)
                            continue;

                        tempBuilder.Font.Size = 12;
                        tempBuilder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
                        tempBuilder.Write(footerNode.FirstChild.InnerText);
                    }
                }

                //開始輸出正文
                tempBuilder.MoveToDocumentStart();
                //抓取SE檔中文件內容的所有Node
                var contentNodes = tempHtmlDoc.DocumentNode.SelectSingleNode("//div[@class='PageContent']")?.ChildNodes;
                if (contentNodes == null)
                    continue;
                foreach (var contentNode in contentNodes)
                {
                    //如果Node為換行符號(\n)或註解(<!--123-->)則跳過
                    if (contentNode.OriginalName == "#text" || contentNode.OriginalName == "#comment" || contentNode.GetAttributeValue("class", "").Contains("noprint"))
                        continue;
                    if (contentNode.Attributes.Contains("data-speed-id"))
                    {
                        var fontSize = GetFontSize(contentNode);
                        tempBuilder.Font.Size = fontSize;
                        textBefore = contentNode.GetAttributeValue("data-speed-before", "");
                        textAfter = contentNode.GetAttributeValue("data-speed-after", "");
                        tempBuilder.ParagraphFormat.LeftIndent = textBefore.Length * fontSize;
                        tempBuilder.ParagraphFormat.FirstLineIndent = textBefore.Length * fontSize * -1;
                        //Add by Stan 20171211: 增加行距設定
                        tempBuilder.ParagraphFormat.LineSpacingRule = LineSpacingRule.Exactly;
                        tempBuilder.ParagraphFormat.LineSpacing = fontSize;
                        tempBuilder.Writeln($"{textBefore}{contentNode.InnerText}{textAfter}");
                        tempBuilder.ParagraphFormat.ClearFormatting();

                    }
                    else
                    {
                        tempBuilder.Font.Size = 12;
                        contentNode.SetAttributeValue("class", $"printapprovallist {contentNode.GetAttributeValue("class", "")}");
                        var table = contentNode.SelectSingleNode("table");
                        if (table == null)
                            continue;
                        table.SetAttributeValue("class", $"printapprovallist {table.GetAttributeValue("class", "")}");
                        table.SetAttributeValue("style", $"width:100%; {table.GetAttributeValue("style", "")}");
                        //Add by Stan 20171211: 增加行距設定
                        tempBuilder.ParagraphFormat.LineSpacingRule = LineSpacingRule.Exactly;
                        tempBuilder.ParagraphFormat.LineSpacing = 12;
                        tempBuilder.Write("");
                        // 有自己增加一些CSS以符合Aspose的規範
                        #region CustomCSS
                        var customCSS = @"<style>
                                            .printapprovallist table tr{
                                                height: auto;
                                            }

                                            .printapprovallist .table-head{
                                              height: auto !important;
                                              }
                                            .printapprovallist .TdAlignCenter{
                                              padding: 4px 5px !important;
                                            }
                                            .printapprovallist .RectangleBox tr{
                                              height: auto;
                                            }
                                            .printapprovallist .RectangleBox td{
                                                margin-top:0px;
                                                margin-bottom:0px;
                                                padding:0px;
                                                color:red;
                                                border-collapse: unset;
                                                vertical-align: middle !important;
                                                border: 0px;
                                            }
                                            .printapprovallist .RectangleBox {
                                                border: 1px solid red;
                                                /*border-style:solid;
                                                border-width:1px;
                                                border-color:red;*/
                                                display: inline-block;
                                                border-collapse: initial;
                                                border-spacing: 2px;
                                            }
                                            .printapprovallist table.RectangleBox{
                                                text-align: center; /*Add by Stan: 印章置中*/
                                                margin: 0 auto; /*Add by Stan: 印章位置置中*/
                                            }
                                            .printapprovallist table.RectangleBox td{
                                                text-align: left; /*Add by Stan: 印章文字置左*/
                                            }
                                            .printapprovallist .RectangleBox p {
                                                margin-left: 5px;
                                                margin-right: 5px;
                                            }
                                            .printapprovallist .TdDescription{
                                              vertical-align: top !important;
                                            }
                                            .printapprovallist .TdAlignCenter{
                                              vertical-align: top !important; /*Add by Stan: 印章置上*/
                                            }
                                            .printapprovallist .chapter{
                                                display: inline-block;
                                            }

                                            .printapprovallist .TimeBox {
                                                /*margin-top: 2px;*/
                                                text-align: center;
                                            }

                                            .printapprovallist .chapter:hover .closebutton{
                                                display: block;
                                            }
                                            div[data-speed-id=seal-box] div{
                                                color:black;
                                            }
                                            div[data-speed-id=seal-box] table{
                                                margin: 0 auto;
                                            }
                                            div[data-speed-id=seal-box] table tbody{

                                            }
                                            div[data-speed-id=seal-box] table tbody tr{
                                                color:black;
                                            }
                                            div[data-speed-id=seal-box] table tbody tr td{
                                                border:0;
                                                padding:0;
                                                vertical-align:middle;
                                                text-align:center; /*時間置中*/
                                            }
                                            div[data-speed-id=seal-box] [data-speed-id=seal-table] tbody{

                                            }
                                            div[data-speed-id=seal-box] [data-speed-id=seal-table] tbody tr{
                                                color:red;
                                            }
                                            div[data-speed-id=seal-box] > table > tbody > tr:last-child > td {
                                                min-width:100px;
                                            }
                                            div[data-speed-id=seal-box] [data-speed-id=seal-table] tbody tr td{
                                                border:0;
                                                padding:0;
                                                vertical-align:middle;
                                            }
                                            table[data-speed-id=seal-table] {
                                                border:1px solid red !important;
                                                width: 110%;
                                            }
                                          </style>";
                        #endregion
                        //Add by Stan 20180424: 將章的部分轉成橫式輸出
                        //將章根據估算的寬度塞在同一行中，每一行為獨立table(因為可能每行塞的章不一樣多)
                        var tableHtmlDoc = new HtmlDocument();
                        tableHtmlDoc.LoadHtml(EscapeHtml(table.InnerHtml));
                        //算出每個col的寬度
                        var cols = tableHtmlDoc.DocumentNode.SelectSingleNode("//colgroup")?.ChildNodes;
                        var colWidth = new List<double>();
                        if (cols != null)
                        {
                            foreach (var col in cols)
                            {
                                var style = GetStyles(col, tempBuilder);
                                if (style.ContainsKey("width"))
                                    colWidth.Add(Convert.ToDouble(style["width"]));
                                else
                                    colWidth.Add(0);
                            }
                        }
                        //取出所有含有stamp的td
                        var tdWithStamps = tableHtmlDoc.DocumentNode.SelectNodes("//td[descendant::div[@data-speed-id='seal-box']]");
                        if (tdWithStamps != null)
                        {
                            foreach (var td in tdWithStamps)
                            {
                                //算自己td的寬度
                                var index = td.ParentNode.ChildNodes.IndexOf(td);
                                var colspan = Convert.ToInt32(td.GetAttributeValue("colspan", "0"));
                                var tdWidth = 0D;
                                for (var i = index; i < index + colspan; i++)
                                    tdWidth += colWidth[i];
                                //開始處理章
                                var stamps = td.SelectNodes(".//div[@data-speed-id='seal-box']");
                                if (stamps == null)
                                    continue; //應該是不可能啦
                                var stampContainer = HtmlNode.CreateNode("<div></div>");
                                var stampTable = HtmlNode.CreateNode("<table></table>");
                                var stampTr = HtmlNode.CreateNode("<tr></tr>");
                                //剩餘可用寬度
                                var restWidth = tdWidth;
                                foreach (var stamp in stamps)
                                {
                                    var stampTd = HtmlNode.CreateNode("<td style='border:none;'></td>");
                                    stampTd.InnerHtml = stamp.OuterHtml;
                                    //取出Stamp內的文字來估算寬度
                                    var innerText = stamp.SelectSingleNode(".//table[@data-speed-id='seal-table']")?.InnerText?.Replace("&nbsp;", "") ?? "";
                                    var estimateWidth = (innerText.Length - ReturnCleanASCII(innerText).Length / 2) * 9.75; //估算先抓一個字13px(9.75pt)
                                                                                                                            //CSS設定最小100px(75pt)，章的寬度110%
                                    if (estimateWidth < 75)
                                        estimateWidth = 75;
                                    restWidth -= estimateWidth;
                                    //如果剩餘寬度是負數，就換一行 (但必須至少有一個章才換行)
                                    if (restWidth < 0 && stampTr.HasChildNodes)
                                    {
                                        restWidth = tdWidth - estimateWidth;
                                        stampTable.AppendChild(stampTr);
                                        stampTr = HtmlNode.CreateNode("<tr></tr>");
                                        stampContainer.AppendChild(stampTable);
                                        stampTable = HtmlNode.CreateNode("<table></table>");
                                    }
                                    stampTr.AppendChild(stampTd);
                                }
                                stampTable.AppendChild(stampTr);
                                stampContainer.AppendChild(stampTable);
                                stamps.First().ParentNode.InnerHtml = stampContainer.OuterHtml;
                            }
                        }

                        table.InnerHtml = tableHtmlDoc.DocumentNode.OuterHtml;
                        tempBuilder.InsertHtml(_tableCSS + customCSS + table.OuterHtml, true);
                        tempBuilder.ParagraphFormat.ClearFormatting();
                    }
                }

                //Add by Stan 20171116: 重算TABLE的寬度
                _htmlDoc = tempHtmlDoc;
                _tableList = StoreTableWidthData(tempBuilder);

                var tables = tempDoc.GetChildNodes(NodeType.Table, true);
                ResetTableWidth(tables);

                resultDocs.Add(tempDoc);
            }
            return resultDocs;
        }
        private void Process(bool noFooter = false)
        {
            // 設定Document的中英文預設字體
            _doc.Styles.DefaultParagraphFormat.Style.Font.NameFarEast = DefaultFarEastFont;
            _doc.Styles.DefaultParagraphFormat.Style.Font.NameAscii = DefaultAsciiFont;
            _doc.Styles.DefaultParagraphFormat.Style.Font.Size = DefaultFontSize;
            _doc.Styles.DefaultParagraphFormat.KeepTogether = false;
            _doc.Styles.DefaultParagraphFormat.KeepWithNext = false;
            // Add by Stan 20171225: 取消中/英文/符號/數字 之間的空白
            _doc.Styles.DefaultParagraphFormat.AddSpaceBetweenFarEastAndAlpha = false;
            _doc.Styles.DefaultParagraphFormat.AddSpaceBetweenFarEastAndDigit = false;
            // Add by Stan 20171113: DefaultFont需要一併設定 PDF才能正確換行
            _doc.Styles.DefaultFont.NameFarEast = DefaultFarEastFont;
            _doc.Styles.DefaultFont.NameAscii = DefaultAsciiFont;
            // 從SE檔讀取頁面基本配置
            var pageOrientation = _htmlDoc.DocumentNode.FirstChild.GetAttributeValue("class", "Portrait").Contains("Portrait") ? "Portrait" : "Landscape";
            var pageSize = _htmlDoc.DocumentNode.FirstChild.GetAttributeValue("data-speed-pagesize", "A4");
            var pageHeaderSpacing = double.Parse(_htmlDoc.DocumentNode.FirstChild.GetAttributeValue("data-speed-headerspacing", "20"));
            var pageFooterSpacing = double.Parse(_htmlDoc.DocumentNode.FirstChild.GetAttributeValue("data-speed-footerspacing", "20"));
            var pageLeftSpacing = double.Parse(_htmlDoc.DocumentNode.FirstChild.GetAttributeValue("data-speed-leftspacing", "25"));
            var pageRightSpacing = double.Parse(_htmlDoc.DocumentNode.FirstChild.GetAttributeValue("data-speed-rightspacing", "25"));
            var pageGutterLineSpacing = double.Parse(_htmlDoc.DocumentNode.FirstChild.GetAttributeValue("data-speed-gutterlinespacing", "5")) - 5;


            //設定頁面配置方式 (Aspose單位為Point, SE單位為mm, 所以需要轉換)
            _builder.CurrentSection.PageSetup.Orientation = (Orientation)Enum.Parse(typeof(Orientation), pageOrientation);
            _builder.CurrentSection.PageSetup.PaperSize = (PaperSize)Enum.Parse(typeof(PaperSize), pageSize);
            _builder.CurrentSection.PageSetup.TopMargin = ConvertUtil.MillimeterToPoint(pageHeaderSpacing);
            _builder.CurrentSection.PageSetup.BottomMargin = ConvertUtil.MillimeterToPoint(pageFooterSpacing);
            _builder.CurrentSection.PageSetup.LeftMargin = ConvertUtil.MillimeterToPoint(pageLeftSpacing);
            _builder.CurrentSection.PageSetup.RightMargin = ConvertUtil.MillimeterToPoint(pageRightSpacing);
            //Add by Stan: 增加頁首頁尾距離設定，至少需保留11pt(0.39cm)，否則頁首頁尾內容會被切到
            _builder.CurrentSection.PageSetup.HeaderDistance = 11;
            var footerDistance = (ConvertUtil.MillimeterToPoint(pageFooterSpacing) - 12) / 2D;
            if (footerDistance < 11)
                footerDistance = 11;
            _builder.CurrentSection.PageSetup.FooterDistance = footerDistance;
            //第一頁的頁首頁尾與其他頁不同
            _builder.CurrentSection.PageSetup.DifferentFirstPageHeaderFooter = true;
            //插入頁首
            InsertHeader();
            //插入頁尾
            if (!noFooter)
                InsertFooter(pageGutterLineSpacing);

            //Add by Stan 20170509: 插入浮水印
            InsertWatermark();

            //Add by Stan 20170624: 插入OCSeal (暫時寫法)
            if (!string.IsNullOrEmpty(_OCSealCSS))
            {
                InsertOCSeal();
            }

            //開始輸出正文
            _builder.MoveToDocumentStart();
            //抓取SE檔中文件內容的所有Node
            var contentNodes = _htmlDoc.DocumentNode.SelectSingleNode("//div[@class='ContentEditor']").ChildNodes;
            foreach (var contentNode in contentNodes)
            {
                //如果Node為換行符號(\n)或註解(<!--123-->)則跳過
                if (contentNode.OriginalName == "#text" || contentNode.OriginalName == "#comment" || contentNode.GetAttributeValue("class", "") == "noprint")
                    continue;

                //判斷是否需要使用文字方塊(FloatItem)
                if (IsAbsolute(contentNode))
                {
                    var style = GetStyles(contentNode);
                    var textbox = new Shape(_doc, ShapeType.TextBox);
                    //_doc.FirstSection.Body.FirstParagraph.AppendChild(textbox);
                    _builder.InsertNode(textbox);
                    // Edit by Stan 20170502: 如果缺少屬性先從Attribute抓，如果有Style則在取代
                    var styleWidth = ConvertUtil.MillimeterToPoint(Convert.ToDouble(contentNode.GetAttributeValue("data-speed-width", "0")));
                    var styleTop = ConvertUtil.MillimeterToPoint(Convert.ToDouble(contentNode.GetAttributeValue("data-speed-top", "0"))) - _builder.CurrentSection.PageSetup.TopMargin;
                    var styleLeft = $"{Convert.ToDouble(contentNode.GetAttributeValue("data-speed-left", "0")) / 210 * 100}%";
                    if (style.ContainsKey("width"))
                    {
                        styleWidth = Convert.ToDouble(style["width"]);
                    }
                    if (style.ContainsKey("top"))
                    {
                        styleTop = Convert.ToDouble(style["top"]);
                    }
                    if (style.ContainsKey("left"))
                    {
                        styleLeft = style["left"];
                    }
                    // 抓是否有border，目前先簡單判斷一下
                    // border-width: 1px; border-style: dashed;
                    if (style.ContainsKey("border-style"))
                    {
                        textbox.Stroked = true;
                        //只先判斷是否為虛線，其他的樣式等有需求再說~~
                        textbox.Stroke.DashStyle = style["border-style"] == "dashed" ? DashStyle.Dash : DashStyle.Default;
                        textbox.StrokeWeight = style.ContainsKey("border-width") ? ConvertUtil.PixelToPoint(Convert.ToDouble(style["border-width"])) : 0.05;
                        textbox.Stroke.On = true;
                        textbox.Stroke.Opacity = 1;
                        textbox.Stroke.Color = Color.Black;
                    }
                    else
                    {
                        textbox.Stroked = false;
                        textbox.StrokeWeight = 0.05;
                        textbox.Stroke.On = false;
                        textbox.Stroke.Opacity = 0;
                        textbox.Stroke.Color = Color.Transparent;
                    }
                    // 如果都抓不到寬度則從孩子抓最寬
                    if (styleWidth == 0D)
                    {
                        styleWidth = contentNode.ChildNodes.Select(node => ConvertUtil.MillimeterToPoint(Convert.ToDouble(node.GetAttributeValue("data-speed-width", "0")))).Concat(new[] { styleWidth }).Max();
                    }
                    if (style.ContainsKey("height"))
                        textbox.Height = Convert.ToDouble(style["height"]);
                    textbox.Width = styleWidth;
                    textbox.Top = styleTop;
                    textbox.Left = GetRelativeLeftFromPage(styleLeft);
                    textbox.WrapType = WrapType.None;
                    textbox.BehindText = true;
                    textbox.TextBox.FitShapeToText = true;
                    textbox.TextBox.InternalMarginBottom = 0;
                    textbox.TextBox.InternalMarginLeft = 0;
                    textbox.TextBox.InternalMarginRight = 0;
                    textbox.TextBox.InternalMarginTop = 0;
                    textbox.FillColor = Color.Transparent;
                    InsertFloatItem(contentNode, textbox);
                }
                else
                    InsertItem(contentNode);
            }

            //插入騎縫章
            //Add by Stan: 如果沒有Footer則不輸出騎縫章
            if (_sealImage != null && !noFooter)
            {
                var max = pageLeftSpacing + pageRightSpacing - 10;
                var leftMaxWidth = ConvertUtil.PointToPixel(ConvertUtil.MillimeterToPoint(pageLeftSpacing - 5));
                GetRotateSeal(_sealImage, ConvertUtil.PointToPixel(ConvertUtil.MillimeterToPoint(max)), leftMaxWidth);
                InsertSeal(_doc.FirstSection.HeadersFooters[HeaderFooterType.FooterFirst]);
                InsertSeal(_doc.FirstSection.HeadersFooters[HeaderFooterType.FooterPrimary]);
            }
            //Add by Stan 20171106: 表格項目符號處理
            _builder.MoveToDocumentEnd();
            //TR高度調整 => WORD的TR高度是不包含padding跟spacing的，跟HTML相反，所以這裡必須額外扣掉
            AdjustRowHeight(_doc.GetChildNodes(NodeType.Row, true));
            ProcessCell(_doc.GetChildNodes(NodeType.Cell, true));
            //Add by Stan 20180202: 如果整份文只有一個Section，那就刪除最後的SectionBreak (讓分頁盡量與製作一致)
            _builder.MoveToDocumentEnd();
            if (_builder.CurrentParagraph.GetText() == ControlChar.SectionBreak && _doc.Sections.Count == 1)
            {
                _builder.CurrentParagraph.Remove();
            }

            //更新功能變數
            _doc.UpdateFields();
            //Add by Stan 20171116: 重算TABLE的寬度 (20180514增加rowspan處理)
            _tableList = StoreTableWidthData();

            var tables = _doc.GetChildNodes(NodeType.Table, true);
            ResetTableWidth(tables);
        }
        private void ResetTableWidth(NodeCollection tables)
        {
            for (var index = 0; index < tables.Count; index++)
            {
                var table = (Aspose.Words.Tables.Table)tables[index];
                if (index >= _tableList.Count)
                    break;
                var tableData = _tableList[index];
                if (table.Rows.Count == tableData.SplitRows.Count)
                {
                    for (var i = 0; i < tableData.SplitRows.Count; i++)
                    {
                        table.Rows[i].RowFormat.AllowBreakAcrossPages = tableData.SplitRows[i];
                    }
                }

                //if (table.StyleName == "div_|data-speed-id=seal-box_table")
                //{
                //    foreach (Row row in table.Rows)
                //    {
                //        row.RowFormat.AllowBreakAcrossPages = false;
                //    }
                //}

                //如果沒有CellWidth的資料就不用跑下面的迴圈了
                if (tableData.CellWidths.Count == 0)
                    continue;

                var cells = table.GetChildNodes(NodeType.Cell, true);
                var counter = 0;
                foreach (Cell cell in cells.OfType<Cell>())
                {
                    if (!cell.ParentRow.ParentTable.Equals(table))
                    {
                        continue;
                    }

                    var rowIndexInTable = cell.ParentRow.ParentTable.IndexOf(cell.ParentRow);
                    var cellIndexInRow = cell.ParentRow.IndexOf(cell);
                    if (cell.GetText() == "\a" && cell.CellFormat.VerticalMerge == CellMerge.Previous)
                    {
                        //抓出合併的前一格的寬度，並設定為一樣的，否則會造成表格跑版
                        if (rowIndexInTable > 0 && cellIndexInRow > -1)
                        {
                            if (table.Rows[rowIndexInTable - 1].Cells.Count == cell.ParentRow.Cells.Count)
                            {
                                var previousCell = table.Rows[rowIndexInTable - 1].Cells[cellIndexInRow];
                                if (previousCell.CellFormat.VerticalMerge != CellMerge.None)
                                    cell.CellFormat.Width = previousCell.CellFormat.Width;
                            }
                            else //上一行有colspan的情況，需要找到正確的cell
                            {
                                var firstRow = table.Rows[rowIndexInTable - 1].Cells.ToArray()
                                    .Where(c => c.CellFormat.VerticalMerge != CellMerge.None).ToList();

                                //var thisRow = cell.ParentRow.Cells.ToArray()
                                //    .Where(c => c.CellFormat.VerticalMerge == CellMerge.Previous).ToList();//CellMerge.First不用算，他是下一行才需要煩惱的
                                var thisRow = new List<Cell>();
                                //不用處理水平合併的情況
                                if (table.Rows[rowIndexInTable - 1].Cells.Count == cell.ParentRow.Cells.Count)
                                {
                                    for (var i = 0; i < table.Rows[rowIndexInTable - 1].Cells.Count; i++)
                                    {
                                        var previousRowCell = table.Rows[rowIndexInTable - 1].Cells[i];
                                        var currentRowCell = cell.ParentRow.Cells[i];
                                        //上一行有merge且寬度相同(視為同一格)，不論這行是否有merge都須補回
                                        if (previousRowCell.CellFormat.VerticalMerge != CellMerge.None)
                                        {
                                            thisRow.Add(currentRowCell);
                                        }
                                    }
                                }
                                //需要處理水平合併的情況
                                else
                                {
                                    var previousRowEnumerator =
                                        table.Rows[rowIndexInTable - 1].Cells.GetEnumerator();
                                    var currentRowEnumerator = cell.ParentRow.Cells.GetEnumerator();
                                    do
                                    {
                                        Cell previousRowCell;
                                        if (previousRowEnumerator.MoveNext())
                                            previousRowCell = (Cell)previousRowEnumerator.Current;
                                        else
                                            break;

                                        Cell currentRowCell;
                                        if (currentRowEnumerator.MoveNext())
                                            currentRowCell = (Cell)currentRowEnumerator.Current;
                                        else
                                            break;

                                        //上一行有merge且寬度相同(視為同一格)，不論這行是否有merge都須補回
                                        //Math.Abs(currentRowCell.CellFormat.Width - previousRowCell.CellFormat.Width) < 0.1 &&
                                        if (previousRowCell.CellFormat.VerticalMerge != CellMerge.None ||
                                            currentRowCell.CellFormat.VerticalMerge == CellMerge.Previous)
                                        {
                                            thisRow.Add(currentRowCell); //無論寬度是否一致都該加入
                                        }

                                        if (Math.Abs(currentRowCell.CellFormat.Width - previousRowCell.CellFormat.Width) >
                                            0.1 &&
                                            currentRowCell.CellFormat.VerticalMerge != CellMerge.Previous)
                                        {
                                            //目前行的比較大，把上一行的往後推
                                            if (currentRowCell.CellFormat.Width > previousRowCell.CellFormat.Width)
                                            {
                                                var currentWidth =
                                                    currentRowCell.CellFormat.Width - previousRowCell.CellFormat.Width;
                                                while (Math.Abs(currentWidth) > 0.1 &&
                                                       previousRowEnumerator.MoveNext())
                                                {
                                                    currentWidth -= ((Cell)previousRowEnumerator.Current).CellFormat.Width;
                                                }
                                            }
                                            else
                                            {
                                                var currentWidth =
                                                    previousRowCell.CellFormat.Width - currentRowCell.CellFormat.Width;
                                                while (Math.Abs(currentWidth) > 0.1 &&
                                                       currentRowEnumerator.MoveNext())
                                                {
                                                    currentWidth -= ((Cell)currentRowEnumerator.Current).CellFormat.Width;
                                                }
                                            }
                                        }
                                    } while (true);
                                }


                                var indexInThisRow = thisRow.IndexOf(cell);
                                if (firstRow.Count > indexInThisRow && indexInThisRow != -1)
                                    cell.CellFormat.Width = firstRow[indexInThisRow].CellFormat.Width;
                            }
                        }

                        continue;
                    }

                    //var debugger = cell.GetText();
                    if (tableData.CellWidths.Count > counter && tableData.CellWidths[counter] > 0)
                    {
                        //寬度是小數點會有誤差，所以第二行開始，要在最後一個cell補齊寬度
                        if (rowIndexInTable > 0 && cell.IsLastCell)
                        {
                            var firstRowWidth = 0D;
                            foreach (Cell c in table.Rows[0].Cells.ToArray())
                            {
                                firstRowWidth += c.CellFormat.Width;
                            }

                            var currentRowWidth = 0D;
                            for (var i = 0; i < cellIndexInRow; i++)
                            {
                                Cell c = table.Rows[rowIndexInTable].Cells[i];
                                currentRowWidth += c.CellFormat.Width;
                            }

                            cell.CellFormat.Width = firstRowWidth - currentRowWidth;
                        }
                        else
                            cell.CellFormat.Width = tableData.CellWidths[counter];
                    }

                    counter++;
                }
            }
        }
        private void AdjustRowHeight(NodeCollection trs)
        {
            foreach (Row tr in trs.OfType<Row>())
            {
                if (Math.Abs(tr.RowFormat.Height) < 0.1)
                    continue;
                tr.RowFormat.Height -= tr.ParentTable.TopPadding + tr.ParentTable.BottomPadding + 0.75 * 2; //0.75為框線寬度(1px)
            }
        }
        private void ProcessCell(NodeCollection tds)
        {
            foreach (Cell td in tds.OfType<Cell>())
            {
                var tdText = td.GetText().Replace("\a", "");
                if (!_listHtmlData.ContainsKey(tdText))
                    continue;
                var htmlContent = _listHtmlData[tdText];
                //將指標移向該cell
                var cellIndex = td.ParentRow.IndexOf(td);
                var rowIndex = td.ParentRow.ParentTable.IndexOf(td.ParentRow);
                var tableIndex = _doc.GetChildNodes(NodeType.Table, true).IndexOf(td.ParentRow.ParentTable);
                _builder.MoveToCell(tableIndex, rowIndex, cellIndex, 0);
                //有可能會吃到前段table的margin-bottom，所以這邊先歸0
                _builder.ParagraphFormat.SpaceBefore = 0;
                _builder.ParagraphFormat.SpaceAfter = 0;
                //去除原本的標記
                td.FirstParagraph.RemoveAllChildren();

                //前處理
                var isListItem = false;
                var classString = htmlContent.GetAttributeValue("class", "");
                var style = GetStyles(htmlContent);
                if (classString.Contains("FormTitleJustify") || classString.Contains("TdAlignVerticalCenter"))
                {
                    td.CellFormat.VerticalAlignment = CellVerticalAlignment.Center;
                }

                if (style.ContainsKey("vertical-align"))
                {
                    switch (style["vertical-align"])
                    {
                        case "middle":
                            td.CellFormat.VerticalAlignment = CellVerticalAlignment.Center;
                            break;
                        case "bottom":
                        case "text-bottom":
                        case "sub":
                            td.CellFormat.VerticalAlignment = CellVerticalAlignment.Bottom;
                            break;
                        default:
                            td.CellFormat.VerticalAlignment = CellVerticalAlignment.Top;
                            break;
                    }
                }

                //如果TD內容是含有表格的話必須塞回原本的htmlDoc，不然算表格寬度的時候會漏算
                var htmlTd = _htmlDoc.DocumentNode.SelectSingleNode($".//td[contains(., '{tdText}')]") ?? htmlContent;
                if (htmlContent.Descendants("table").Any() && htmlTd != null)
                {
                    htmlTd.InnerHtml = htmlContent.InnerHtml;
                    td.FirstParagraph.ParagraphFormat.Alignment = ParagraphAlignment.Left;
                }

                //TODO:處理#text與div混合情況

                if (htmlContent.HasChildNodes &&
                    htmlContent.ChildNodes.Any(c => c.Name == "div" || c.Name == "table" || c.Name == "ul"))
                {
                    foreach (var htmlNode in htmlContent.ChildNodes)
                    {
                        if (IsSkippableNode(htmlNode))
                        {
                            continue;
                        }

                        if (htmlNode.GetAttributeValue("data-speed-id", "").Contains("paragraph"))
                        {
                            isListItem = true;
                            //設定Indent，讓項目符號排版正確 (WORD會根據這兩項設定去調整LIST的INDENT)
                            td.FirstParagraph.ParagraphFormat.LeftIndent = 0;
                            td.FirstParagraph.ParagraphFormat.FirstLineIndent = -32;
                            InsertItem(htmlNode, false, true, htmlTd);
                            AdjustRowHeight(td.GetChildNodes(NodeType.Row, true));
                        }
                        else if (htmlNode.Name == "table") //CTBC會辦單位，TFC排版用內層表格
                        {
                            InsertItem(htmlNode.ParentNode, false, true, htmlTd);
                            var tdTables = td.Tables;
                            if (tdTables != null)
                            {
                                foreach (Table tdTable in tdTables.ToArray())
                                {
                                    var tdTableCells = tdTable.GetChildNodes(NodeType.Cell, true);
                                    foreach (Cell cell in tdTableCells.OfType<Cell>())
                                    {
                                        cell.CellFormat.SetPaddings(0, 0, 0, 0);
                                    }
                                    ProcessCell(tdTableCells);

                                    tdTable.CellSpacing = 0;
                                    tdTable.TopPadding = 0;
                                    tdTable.LeftPadding = 0;
                                    tdTable.RightPadding = 0;
                                    tdTable.BottomPadding = 0;
                                    AdjustRowHeight(tdTable.Rows);
                                }
                            }
                        }
                        else if (htmlNode.GetAttributeValue("data-speed-id", "") == "exporterStampContainer")
                        {
                            InsertItem(htmlNode, false, true, htmlTd);
                            var tdTables = td.Tables;
                            if (tdTables != null)
                            {
                                foreach (Table tdTable in tdTables.ToArray())
                                {
                                    var stampTables = tdTable.GetChildNodes(NodeType.Table, true);
                                    foreach (Table stampTable in stampTables.OfType<Table>())
                                    {
                                        if (((Cell)stampTable.ParentNode).ParentRow.ParentTable == tdTable)
                                        {
                                            stampTable.AutoFit(AutoFitBehavior.AutoFitToContents);
                                            stampTable.TopPadding = 0;
                                            stampTable.LeftPadding = 0;
                                            stampTable.RightPadding = 0;
                                            stampTable.BottomPadding = 0;
                                            stampTable.CellSpacing = 0;
                                            foreach (Cell cell in stampTable.GetChildNodes(NodeType.Cell, true).OfType<Cell>())
                                            {
                                                var runs = cell.GetChildNodes(NodeType.Run, true);
                                                if (runs == null)
                                                    continue;
                                                var maxFontSize = 0D;
                                                foreach (Run run in runs.OfType<Run>())
                                                {
                                                    if (run.Font.Size > maxFontSize)
                                                        maxFontSize = run.Font.Size;
                                                }

                                                foreach (Paragraph paragraph in cell.Paragraphs.ToArray())
                                                {
                                                    paragraph.ParagraphFormat.LineSpacingRule = LineSpacingRule.Exactly;
                                                    paragraph.ParagraphFormat.LineSpacing = maxFontSize;
                                                }
                                            }
                                        }
                                    }
                                    tdTable.CellSpacing = 0;
                                    tdTable.AutoFit(AutoFitBehavior.AutoFitToContents);
                                }
                            }
                        }
                        else
                        {
                            InsertItem(htmlNode, false, true, htmlTd);
                        }
                    }
                }
                else if (!IsSkippableNode(htmlContent))
                {
                    InsertItem(htmlContent, false, true, htmlTd);
                }

                //去除最後多餘的換行
                for (var i = td.Paragraphs.Count - 1; i >= 0; i--)
                {
                    //cell中斷符號(\a)不處理
                    if (td.Paragraphs[i].GetText() == "\a")
                        continue;
                    //多餘的換行符號(paragraph break)需要刪除
                    if (td.Paragraphs[i].GetText() == "\r" && !isListItem) //!td.Paragraphs[i].IsListItem
                    {
                        td.Paragraphs.RemoveAt(i);
                        continue;
                    }

                    //最後一個項目的換行符號也要刪掉，但須要補回樣式 + 調整該ROW的高度
                    var newLineIndex = td.Paragraphs[i].GetText().LastIndexOf("\r", StringComparison.Ordinal);
                    //最後含有\r且是項目符號的paragraph
                    if (newLineIndex != -1 && isListItem) //td.Paragraphs[i].IsListItem
                    {
                        dynamic para = td.Paragraphs[i].Clone(true);
                        var original = td.Paragraphs[i].GetText();
                        var target = original.Remove(newLineIndex);
                        //SOURCE:https://apireference.aspose.com/net/words/aspose.words/range/methods/replace
                        td.Paragraphs[i].Range.Replace($"{target}&p", target, new Aspose.Words.Replacing.FindReplaceOptions());
                        //Replace之後格式會跑掉，要補回來
                        td.Paragraphs[i].ListFormat.List = para.ListFormat.List;
                        td.Paragraphs[i].ListFormat.ListLevelNumber = para.ListFormat.ListLevelNumber;
                        td.Paragraphs[i].ParagraphFormat.Alignment = para.ParagraphFormat.Alignment;
                        td.Paragraphs[i].ParagraphFormat.LineSpacing = para.ParagraphFormat.LineSpacing;
                        td.Paragraphs[i].ParagraphFormat.LineSpacingRule = para.ParagraphFormat.LineSpacingRule;
                        td.Paragraphs[i].ParagraphFormat.Style = para.ParagraphFormat.Style;
                        td.Paragraphs[i].ParagraphFormat.SpaceBefore = para.ParagraphFormat.SpaceBefore;
                        td.Paragraphs[i].ParagraphFormat.SpaceBeforeAuto = para.ParagraphFormat.SpaceBeforeAuto;
                        td.Paragraphs[i].ParagraphFormat.SpaceAfter = para.ParagraphFormat.SpaceAfter;
                        td.Paragraphs[i].ParagraphFormat.SpaceAfterAuto = para.ParagraphFormat.SpaceAfterAuto;
                        td.Paragraphs[i].ParagraphFormat.LeftIndent = para.ParagraphFormat.LeftIndent;

                        //開始調整高度
                        var settingheight = td.ParentRow.RowFormat.Height;
                        //算出目前ROW的高度，只是概算而已，可能會有誤差
                        var currentHeight = CalculateRowHeight(td.ParentRow);
                        //如果目前高度大於設定高度就強制縮小，否則有可能會遇到跨頁表格高度的問題
                        if (currentHeight > settingheight)
                        {
                            td.ParentRow.RowFormat.HeightRule = HeightRule.AtLeast;
                            td.ParentRow.RowFormat.Height = 1;
                        }

                        break;
                    }
                }
            }
        }
        private bool IsSkippableNode(HtmlNode htmlNode)
        {
            return htmlNode.Name == "#text" && SkipCharList.Any(htmlNode.InnerText.Contains);
        }

        private void InsertOCSeal()
        {
            var sealName = String.Empty;
            switch (_outKind)
            {
                case 1://正本
                case 3://主持人
                case 4://出席者
                    sealName = "正本";
                    break;
                case 2://副本
                case 5://列席者
                    sealName = "副本";
                    break;
                case 7://抄本
                    sealName = "抄本";
                    break;
                default:
                    return;
            }

            var style = GetStyles(_OCSealCSS);
            var ocSeal = new Shape(_doc, ShapeType.TextBox);
            ocSeal.Width = Convert.ToDouble(style["width"]);
            ocSeal.Height = Convert.ToDouble(style["height"]);
            ocSeal.Top = Convert.ToDouble(style["top"]);
            ocSeal.Left = Convert.ToDouble(style["left"]);
            ocSeal.RelativeHorizontalPosition = RelativeHorizontalPosition.Page;
            ocSeal.RelativeVerticalPosition = RelativeVerticalPosition.Page;
            ocSeal.WrapType = WrapType.None;
            ocSeal.BehindText = true;

            ocSeal.Font.Size = Convert.ToDouble(style["font-size"]);
            ocSeal.Font.Bold = style["font-weight"] == "bold";
            ocSeal.Font.Color = Color.FromName(style["color"]);
            var border = style["border"].Split(' ');
            ocSeal.StrokeColor = Color.FromName(border[2]);
            ocSeal.StrokeWeight = ConvertUtil.PixelToPoint(Convert.ToDouble(border[0].Replace("px", "")));
            ocSeal.TextBox.InternalMarginBottom = 0;
            ocSeal.TextBox.InternalMarginLeft = 7;
            ocSeal.TextBox.InternalMarginRight = 0;
            ocSeal.TextBox.InternalMarginTop = 2.5;
            var paragraph = new Paragraph(_doc);
            var runText = new Run(_doc);
            runText.Font.Size = Convert.ToDouble(style["font-size"]);
            runText.Font.Bold = style["font-weight"] == "bold";
            runText.Font.Color = Color.FromName(style["color"]);
            runText.Text = sealName;
            paragraph.AppendChild(runText);
            ocSeal.AppendChild(paragraph);

            //_builder.MoveToHeaderFooter(HeaderFooterType.HeaderPrimary);
            //_builder.InsertNode(ocSeal.Clone(true));
            _builder.MoveToHeaderFooter(HeaderFooterType.HeaderFirst);
            _builder.InsertNode(ocSeal.Clone(true));
            //_builder.MoveToHeaderFooter(HeaderFooterType.HeaderEven);
            //_builder.InsertNode(ocSeal.Clone(true));
        }

        private void InsertWatermark()
        {
            if (_watermarkImage != null)
            {
                /*
                     ActiveDocument.Sections(1).Range.Select
                    ActiveWindow.ActivePane.View.SeekView = wdSeekCurrentPageHeader
                    Selection.HeaderFooter.Shapes.AddPicture(FileName:= _
                        "C:\Users\j2457\Desktop\文化部製作需協助處理問題\5.請協助修改匯出公文要能夠帶出浮水印\watermark.png", _
                        LinkToFile:=False, SaveWithDocument:=True).Select
                    Selection.ShapeRange.Name = "WordPictureWatermark372105953"
                    Selection.ShapeRange.PictureFormat.Brightness = 0.85
                    Selection.ShapeRange.PictureFormat.Contrast = 0.15
                    Selection.ShapeRange.LockAspectRatio = True
                    Selection.ShapeRange.Height = CentimetersToPoints(2.75)
                    Selection.ShapeRange.Width = CentimetersToPoints(2.8)
                    Selection.ShapeRange.WrapFormat.AllowOverlap = True
                    Selection.ShapeRange.WrapFormat.Side = wdWrapNone
                    Selection.ShapeRange.WrapFormat.Type = 3
                    Selection.ShapeRange.RelativeHorizontalPosition = _
                        wdRelativeVerticalPositionMargin
                    Selection.ShapeRange.RelativeVerticalPosition = _
                        wdRelativeVerticalPositionMargin
                    Selection.ShapeRange.Left = wdShapeCenter
                    Selection.ShapeRange.Top = wdShapeCenter
                    ActiveWindow.ActivePane.View.SeekView = wdSeekMainDocument
                    */
                var watermark = new Shape(_doc, ShapeType.Image);
                watermark.ImageData.SetImage(_watermarkImage);
                // 預設先不調淡
                //watermark.ImageData.Brightness = 0.75;
                //watermark.ImageData.Contrast = 0.3;
                // setting image width and height
                watermark.Width = _watermarkImage.Width;
                watermark.Height = _watermarkImage.Height;
                // Image will be placed center of page
                watermark.RelativeHorizontalPosition = RelativeHorizontalPosition.Page;
                watermark.RelativeVerticalPosition = RelativeVerticalPosition.Page;
                watermark.WrapType = WrapType.None;
                watermark.VerticalAlignment = VerticalAlignment.Center;
                watermark.HorizontalAlignment = HorizontalAlignment.Center;
                watermark.BehindText = true;

                _builder.MoveToHeaderFooter(HeaderFooterType.HeaderPrimary);
                _builder.InsertNode(watermark.Clone(true));
                _builder.MoveToHeaderFooter(HeaderFooterType.HeaderFirst);
                _builder.InsertNode(watermark.Clone(true));
                _builder.MoveToHeaderFooter(HeaderFooterType.HeaderEven);
                _builder.InsertNode(watermark.Clone(true));
            }
        }

        private void InsertSeal(HeaderFooter header)
        {
            var localLeftPath = _sealImageLeftName.Replace("\\", "\\\\");
            var localRightPath = _sealImageRightName.Replace("\\", "\\\\");

            //Insert Left Textboxes AND Field
            var shape = new Shape(_doc, ShapeType.TextBox);
            SealShapeSetting(shape, false);
            shape.VerticalAlignment = VerticalAlignment.Top;
            shape.Left = 14;
            var paragraph = new Paragraph(_doc);
            paragraph.AppendChild(new Run(_doc, ""));
            shape.AppendChild(paragraph);
            header.FirstParagraph.AppendChild(shape);
            InsertFieldIntoLeftShape(shape, 2, localLeftPath);

            shape = new Shape(_doc, ShapeType.TextBox);
            SealShapeSetting(shape, false);
            shape.VerticalAlignment = VerticalAlignment.Center;
            shape.Left = 14;
            paragraph = new Paragraph(_doc);
            paragraph.AppendChild(new Run(_doc, ""));
            shape.AppendChild(paragraph);
            header.FirstParagraph.AppendChild(shape);
            InsertFieldIntoLeftShape(shape, 0, localLeftPath);

            shape = new Shape(_doc, ShapeType.TextBox);
            SealShapeSetting(shape, false);
            shape.VerticalAlignment = VerticalAlignment.Bottom;
            shape.Left = 14;
            paragraph = new Paragraph(_doc);
            paragraph.AppendChild(new Run(_doc, ""));
            shape.AppendChild(paragraph);
            header.FirstParagraph.AppendChild(shape);
            InsertFieldIntoLeftShape(shape, 1, localLeftPath);

            //Insert Right Textboxes AND Field
            shape = new Shape(_doc, ShapeType.TextBox);
            SealShapeSetting(shape, true);
            shape.VerticalAlignment = VerticalAlignment.Top;
            shape.Left = Convert.ToInt32(_builder.PageSetup.PageWidth - ConvertUtil.PixelToPoint(_sealImageRight.Width)) - 14;
            paragraph = new Paragraph(_doc);
            paragraph.AppendChild(new Run(_doc, ""));
            shape.AppendChild(paragraph);
            header.FirstParagraph.AppendChild(shape);
            InsertFieldIntoRightShape(shape, 1, localRightPath);

            shape = new Shape(_doc, ShapeType.TextBox);
            SealShapeSetting(shape, true);
            shape.VerticalAlignment = VerticalAlignment.Center;
            shape.Left = Convert.ToInt32(_builder.PageSetup.PageWidth - ConvertUtil.PixelToPoint(_sealImageRight.Width)) - 14;
            paragraph = new Paragraph(_doc);
            paragraph.AppendChild(new Run(_doc, ""));
            shape.AppendChild(paragraph);
            header.FirstParagraph.AppendChild(shape);
            InsertFieldIntoRightShape(shape, 2, localRightPath);

            shape = new Shape(_doc, ShapeType.TextBox);
            SealShapeSetting(shape, true);
            shape.VerticalAlignment = VerticalAlignment.Bottom;
            shape.Left = Convert.ToInt32(_builder.PageSetup.PageWidth - ConvertUtil.PixelToPoint(_sealImageRight.Width)) - 14;
            paragraph = new Paragraph(_doc);
            paragraph.AppendChild(new Run(_doc, ""));
            shape.AppendChild(paragraph);
            header.FirstParagraph.AppendChild(shape);
            InsertFieldIntoRightShape(shape, 0, localRightPath);
        }
        private void SealShapeSetting(Shape shape, bool isRight)
        {
            shape.TextBox.InternalMarginBottom = 0;
            shape.TextBox.InternalMarginLeft = 0;
            shape.TextBox.InternalMarginRight = 0;
            shape.TextBox.InternalMarginTop = 0;
            shape.Stroked = false;
            shape.StrokeWeight = 0.05;
            shape.Stroke.On = false;
            shape.Stroke.Opacity = 0;
            shape.Stroke.Color = Color.Transparent;
            shape.FillColor = Color.Transparent;
            shape.Width = isRight ? _sealImageRight.Width : _sealImageLeft.Width;
            shape.Height = isRight ? _sealImageRight.Height : _sealImageLeft.Height;
            shape.WrapType = WrapType.None;
            shape.RelativeVerticalPosition = RelativeVerticalPosition.Margin;
            shape.RelativeHorizontalPosition = RelativeHorizontalPosition.Page;
        }
        private void DeleteSealFile()
        {
            if (File.Exists(_sealImageLeftName))
                File.Delete(_sealImageLeftName);
            if (File.Exists(_sealImageRightName))
                File.Delete(_sealImageRightName);
        }
        private void GetRotateSeal(Image sealImage, double maxWidth, double leftMaxWidth)
        {
            var rightMaxWidth = maxWidth - leftMaxWidth;
            // Get Random Value
            System.Security.Cryptography.RNGCryptoServiceProvider rngp = new System.Security.Cryptography.RNGCryptoServiceProvider();
            byte[] rb = new byte[4];
            var angle = 0F;
            rngp.GetBytes(rb);
            angle = BitConverter.ToSingle(rb, 0) % 26;
            if (angle < 0) angle = -angle;
            angle = (angle - 4) / 100F;

            // Get New Rotated Seal
            var rotateSeal = RotateImage(sealImage, angle);
            var height = rotateSeal.Height;
            int leftWidth, rightWidth;
            //應該是要旋轉後再縮圖
            //如果騎縫章寬度大於maxWidth的話，要先縮圖
            if (rotateSeal.Width >= Math.Floor(maxWidth))
            {
                var factor = rotateSeal.Width / maxWidth;
                rotateSeal = ResizeImage(rotateSeal, Convert.ToInt32(rotateSeal.Width / factor), Convert.ToInt32(rotateSeal.Height / factor));
                //縮圖後，代表一定跟最大寬度一致，所以直接指定左右寬度為最大寬度即可
                leftWidth = Convert.ToInt32(leftMaxWidth);
                rightWidth = Convert.ToInt32(rightMaxWidth);
            }
            else
            {
                //沒縮圖代表圖片比最大寬度小，有可能可以左右切
                //切後寬度不得超過leftMaxWidth或rightMaxWidth，超過就要重新切
                //避免無窮迴圈，如果切10次還切不出來，就直接用最大容許寬度
                var count = 0;
                double width;
                do
                {
                    count++;

                    rngp.GetBytes(rb);
                    width = BitConverter.ToDouble(rb, 0) % 61;
                    if (width < 0) width = -width;
                    width = (width + 20) / 100D;

                    //width = random.Next(20, 80) / 100D;
                } while ((rotateSeal.Width * width > leftMaxWidth || rotateSeal.Width * (1 - width) > rightMaxWidth) && count < 10);
                if (count >= 10)
                {
                    leftWidth = Convert.ToInt32(leftMaxWidth);
                    rightWidth = Convert.ToInt32(rightMaxWidth);
                }
                else
                {
                    leftWidth = Convert.ToInt32(rotateSeal.Width * width);
                    rightWidth = Convert.ToInt32(rotateSeal.Width * (1 - width));
                }
            }

            // Get Left Part -> For Right Seal
            var srcBitmap = new Bitmap(rotateSeal);
            var destBitmap = new Bitmap(leftWidth, height);
            var destRect = new Rectangle(0, 0, destBitmap.Width, destBitmap.Height);
            var srcRect = new Rectangle(0, 0, leftWidth, height);
            Graphics.FromImage(destBitmap).DrawImage(srcBitmap, destRect, srcRect, GraphicsUnit.Pixel); // 將剪裁區域繪製目標圖檔
            _sealImageRight = destBitmap;

            // Get Right Part -> For Left Seal
            srcBitmap = new Bitmap(rotateSeal); // bitmap 為你的原圖
            destBitmap = new Bitmap(rightWidth, height);
            destRect = new Rectangle(0, 0, destBitmap.Width, destBitmap.Height); // 你的輸出範圍
            srcRect = new Rectangle(leftWidth, 0, rightWidth, height); // 你的原圖剪裁區域
            Graphics.FromImage(destBitmap).DrawImage(srcBitmap, destRect, srcRect, GraphicsUnit.Pixel); // 將剪裁區域繪製目標圖檔
            _sealImageLeft = destBitmap;

            //Save The Seal
            var fileName = Guid.NewGuid().GetHashCode();
            //modify by stan 20170314 路徑一定要這樣寫，否則會讀不到圖片 (詭異)
            _sealImageLeftName = Path.Combine($"{_savePath}", $"{fileName}_left.png");
            _sealImageRightName = Path.Combine($"{_savePath}", $"{fileName}_right.png");
            _sealImageLeft.Save(_sealImageLeftName);
            _sealImageRight.Save(_sealImageRightName);
        }
        private Bitmap RotateImage(Image bmpSrc, float theta)
        {
            Matrix mRotate = new Matrix();
            mRotate.Translate(bmpSrc.Width / -2, bmpSrc.Height / -2, MatrixOrder.Append);
            mRotate.RotateAt(theta, new System.Drawing.Point(0, 0), MatrixOrder.Append);
            using (GraphicsPath gp = new GraphicsPath())
            {  // transform image points by rotation matrix
                gp.AddPolygon(new System.Drawing.Point[] { new System.Drawing.Point(0, 0), new System.Drawing.Point(bmpSrc.Width, 0), new System.Drawing.Point(0, bmpSrc.Height) });
                gp.Transform(mRotate);
                System.Drawing.PointF[] pts = gp.PathPoints;

                // create destination bitmap sized to contain rotated source image
                Rectangle bbox = boundingBox(bmpSrc, mRotate);
                Bitmap bmpDest = new Bitmap(bbox.Width, bbox.Height);

                using (Graphics gDest = Graphics.FromImage(bmpDest))
                {  // draw source into dest
                    Matrix mDest = new Matrix();
                    mDest.Translate(bmpDest.Width / 2, bmpDest.Height / 2, MatrixOrder.Append);
                    gDest.Transform = mDest;
                    gDest.DrawImage(bmpSrc, pts);
                    return bmpDest;
                }
            }
        }
        private Rectangle boundingBox(Image img, Matrix matrix)
        {
            GraphicsUnit gu = new GraphicsUnit();
            Rectangle rImg = Rectangle.Round(img.GetBounds(ref gu));

            // Transform the four points of the image, to get the resized bounding box.
            System.Drawing.Point topLeft = new System.Drawing.Point(rImg.Left, rImg.Top);
            System.Drawing.Point topRight = new System.Drawing.Point(rImg.Right, rImg.Top);
            System.Drawing.Point bottomRight = new System.Drawing.Point(rImg.Right, rImg.Bottom);
            System.Drawing.Point bottomLeft = new System.Drawing.Point(rImg.Left, rImg.Bottom);
            System.Drawing.Point[] points = new System.Drawing.Point[] { topLeft, topRight, bottomRight, bottomLeft };
            GraphicsPath gp = new GraphicsPath(points, new byte[] { (byte)PathPointType.Start, (byte)PathPointType.Line, (byte)PathPointType.Line, (byte)PathPointType.Line });
            gp.Transform(matrix);
            return Rectangle.Round(gp.GetBounds());
        }
        private void InsertFieldIntoLeftShape(Shape shape, int mod, string fileName)
        {
            _builder.MoveTo(shape.FirstParagraph);
            var field = _builder.InsertField(@"IF ");
            _builder.MoveTo(field.Separator);
            var fieldx = _builder.InsertField("=AND(");
            _builder.MoveTo(fieldx.Separator);
            var field2 = _builder.InsertField("COMPARE ");
            _builder.MoveTo(field2.Separator);
            _builder.InsertField(@"PAGE \* MERGEFORMAT");
            _builder.Write(@" > 1  \* MERGEFORMAT");
            _builder.MoveTo(fieldx.Separator);
            _builder.Write(@",");
            field2 = _builder.InsertField("COMPARE ");
            _builder.MoveTo(field2.Separator);
            var field3 = _builder.InsertField(@"=mod(");
            _builder.MoveTo(field3.Separator);
            _builder.InsertField(@"PAGE \* MERGEFORMAT");
            _builder.Write(@",3)");
            _builder.MoveTo(field2.Separator);
            _builder.Write($@" = {mod}  \* MERGEFORMAT");
            _builder.MoveTo(fieldx.Separator);
            _builder.Write(@")");
            _builder.MoveTo(field.Separator);
            _builder.Write(@" = 1 ");
            _builder.InsertField($@"INCLUDEPICTURE ""{fileName}"" \* MERGEFORMAT");
        }
        private void InsertFieldIntoRightShape(Shape shape, int mod, string fileName)
        {
            _builder.MoveTo(shape.FirstParagraph);
            var field = _builder.InsertField(@"IF ");
            _builder.MoveTo(field.Separator);
            var fieldx = _builder.InsertField("=AND(");
            _builder.MoveTo(fieldx.Separator);
            var field2 = _builder.InsertField("COMPARE ");
            _builder.MoveTo(field2.Separator);
            var fieldy = _builder.InsertField("=mod(");
            _builder.MoveTo(fieldy.Separator);
            _builder.InsertField(@"PAGE \* MERGEFORMAT");
            _builder.Write(@",3)");
            _builder.MoveTo(fieldy.Separator);
            _builder.MoveTo(field2.Separator);
            _builder.Write($@" = {mod} ");
            _builder.MoveTo(fieldx.Separator);
            _builder.Write(@",");
            var field3 = _builder.InsertField("COMPARE ");
            _builder.MoveTo(field3.Separator);
            _builder.InsertField(@"PAGE \* MERGEFORMAT");
            _builder.Write(@" <> ");
            _builder.InsertField(@"SECTIONPAGES \* MERGEFORMAT");
            _builder.MoveTo(field3.Separator);
            _builder.MoveTo(fieldx.Separator);
            _builder.Write(@")");
            _builder.MoveTo(field.Separator);
            _builder.Write(@" = 1 ");
            _builder.InsertField($@"INCLUDEPICTURE ""{fileName}"" \* MERGEFORMAT");
        }

        private void InsertHeader()
        {
            // Add by Stan: 如果找不到相關Section則跳過
            if (_htmlDoc.DocumentNode.SelectNodes("//div[@class='HeaderEditor']/div") == null)
            {
                return;
            }

            ////讀取頁面左邊界
            var leftMargin = _builder.CurrentSection.PageSetup.LeftMargin;
            ////抓出Header相關的Node
            var firstNode = _htmlDoc.DocumentNode.SelectNodes("//div[@class='HeaderEditor']/div")[0];
            //var classificationNumberNode = _htmlDoc.DocumentNode.SelectNodes("//div[@class='HeaderEditor']/div")[1];
            //Header文字距離文件上方的Margin
            var topMargin = ConvertUtil.MillimeterToPoint(double.Parse(firstNode.GetAttributeValue("data-speed-top", "0")));
            //Header文字距離文件左方的Margin
            var headerMargin = ConvertUtil.MillimeterToPoint(double.Parse(firstNode.GetAttributeValue("data-speed-left", leftMargin.ToString())));
            //分類號文字距離文件左方的Margin
            //var classificationNumberLeftMargin = ConvertUtil.MillimeterToPoint(double.Parse(classificationNumberNode.GetAttributeValue("data-speed-left", leftMargin.ToString())));
            //實際換算出來的Header邊界
            var lineIndent = headerMargin - leftMargin;

            ////輸出位置設定
            _builder.CurrentSection.PageSetup.HeaderDistance = topMargin;
            _builder.MoveToHeaderFooter(HeaderFooterType.HeaderFirst);
            _builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            _builder.ParagraphFormat.LeftIndent = lineIndent;
            _builder.ParagraphFormat.RightIndent = lineIndent;

            var textboxPara = new Paragraph(_doc);
            var headerNodes = _htmlDoc.DocumentNode.SelectSingleNode("//div[@class='HeaderEditor']").ChildNodes;
            foreach (var headerNode in headerNodes)
            {
                //如果Node為換行符號(\r\n)或註解(<!--123-->)則跳過
                if (headerNode.OriginalName == "#text" || headerNode.OriginalName == "#comment" || headerNode.GetAttributeValue("class", "").Contains("noprint"))
                    continue;
                //判斷是否需要使用文字方塊(FloatItem)
                if (IsAbsolute(headerNode))
                {
                    var style = GetStyles(headerNode);
                    var textbox = new Shape(_doc, ShapeType.TextBox);
                    // Edit by Stan 20170502: 如果缺少屬性先從Attribute抓，如果有Style則在取代
                    var styleWidth = ConvertUtil.MillimeterToPoint(Convert.ToDouble(headerNode.GetAttributeValue("data-speed-width", "0")));
                    var styleTop = ConvertUtil.MillimeterToPoint(Convert.ToDouble(headerNode.GetAttributeValue("data-speed-top", "0")));
                    var styleLeft = $"{Convert.ToDouble(headerNode.GetAttributeValue("data-speed-left", "0")) / 210 * 100}%";
                    if (style.ContainsKey("width"))
                    {
                        styleWidth = Convert.ToDouble(style["width"]);
                    }
                    if (style.ContainsKey("top"))
                    {
                        styleTop = Convert.ToDouble(style["top"]);
                    }
                    if (style.ContainsKey("left"))
                    {
                        styleLeft = style["left"];
                    }
                    // 如果都抓不到寬度則從孩子抓最寬
                    if (styleWidth == 0D)
                    {
                        styleWidth = headerNode.ChildNodes.Select(node => ConvertUtil.MillimeterToPoint(Convert.ToDouble(node.GetAttributeValue("data-speed-width", "0")))).Concat(new[] { styleWidth }).Max();
                    }
                    textbox.Width = styleWidth;
                    textbox.Top = styleTop;
                    // Edit by Stan 20170502: Header是沒有左右邊界，所以要特別加回去，以防算錯位置
                    textbox.Left = GetRelativeLeftFromPage(styleLeft, true);
                    textbox.WrapType = WrapType.None;
                    textbox.BehindText = true;
                    textbox.TextBox.FitShapeToText = true;
                    textbox.TextBox.InternalMarginBottom = 0;
                    textbox.TextBox.InternalMarginLeft = 0;
                    textbox.TextBox.InternalMarginRight = 0;
                    textbox.TextBox.InternalMarginTop = 0;
                    textbox.Stroked = false;
                    textbox.StrokeWeight = 0.05;
                    textbox.Stroke.On = false;
                    textbox.Stroke.Opacity = 0;
                    textbox.Stroke.Color = Color.Transparent;
                    textbox.RelativeVerticalPosition = RelativeVerticalPosition.Page;
                    textbox.RelativeHorizontalPosition = RelativeHorizontalPosition.Page;
                    textbox.FillColor = Color.Transparent;
                    InsertFloatItem(headerNode, textbox, true);
                    textboxPara.AppendChild(textbox);
                }
                else
                    InsertItem(headerNode);
            }
            _doc.FirstSection.HeadersFooters[0].AppendChild(textboxPara.Clone(true));
        }
        private void InsertFooter(double gutterLineSpacing)
        {
            //第一頁FOOTER
            _builder.MoveToHeaderFooter(HeaderFooterType.FooterFirst);
            _builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            FontSetting(12);
            _builder.Write("第 ");
            _builder.InsertField("PAGE");
            _builder.Write(" 頁\u3000共 ");
            _builder.InsertField("SECTIONPAGES");
            _builder.Write(" 頁");
            //插入裝訂線
            InsertGutter(gutterLineSpacing);

            //每一頁FOOTER
            _builder.MoveToHeaderFooter(HeaderFooterType.FooterPrimary);
            _builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            FontSetting(12);
            _builder.Write("第 ");
            _builder.InsertField("PAGE");
            _builder.Write(" 頁\u3000共 ");
            _builder.InsertField("SECTIONPAGES");
            _builder.Write(" 頁");
            InsertGutter(gutterLineSpacing);

            //插入BarCode
            //Edit by Stan 20171029: 改為支援多個barcode輸出
            var barCodeNodes = _settingXml.SelectNodes("//barcode");
            if (barCodeNodes == null) return;

            foreach (XmlNode barCodeNode in barCodeNodes)
            {
                //移回第一頁頁尾
                _builder.MoveToHeaderFooter(HeaderFooterType.FooterFirst);
                var everyPage = false;
                var barCodeSetting = ParseBarCodeSetting(barCodeNode);
                if (barCodeSetting.ContainsKey("everypage"))
                    everyPage = Convert.ToBoolean(barCodeSetting["everypage"]);
                InsertBarCode(barCodeNode, barCodeSetting);
                //如果是每頁都輸出，就移到主頁尾輸出
                if (everyPage)
                {
                    _builder.MoveToHeaderFooter(HeaderFooterType.FooterPrimary);
                    InsertBarCode(barCodeNode, barCodeSetting);
                }
            }
        }
        private void InsertFloatItem(HtmlNode contentNode, Shape textbox, bool isHeader = false, bool isRecursive = false)
        {
            var nodes = contentNode.ChildNodes;
            if (isHeader)
            {
                textbox.RelativeVerticalPosition = RelativeVerticalPosition.Page;
                if (!isRecursive)
                    nodes = new HtmlNodeCollection(contentNode.ParentNode) { contentNode };
            }
            else
                textbox.RelativeVerticalPosition = RelativeVerticalPosition.Margin;

            foreach (var node in nodes)
            {
                //Modify by Stan 20171203: 修正DI轉SE，HeaderNode會不見(檔號,保存年限)
                //如果有以下條件就跳過
                //1.是註解
                //2.class中有noprint
                //3.內容(InnerText)為空且沒有設定data-speed-id與data-speed-label且沒有其他小孩(其他control)且不是img [DI轉SE會出現這樣的格式]
                if (node.OriginalName == "#comment" ||
                    node.GetAttributeValue("class", "").Contains("noprint") ||
                    node.InnerText.Replace("\n", "").Replace("\r", "").Trim() == String.Empty && node.GetAttributeValue("data-speed-id", "") == "" && node.GetAttributeValue("data-speed-label", "") == "" && (node.HasChildNodes && node.FirstChild.OriginalName == "div") == false && node.OriginalName != "img")
                    continue;

                //Modify by Stan 20180202: 巢狀div優先拆解
                if (node.HasChildNodes && node.FirstChild.OriginalName == "div")
                {
                    InsertFloatItem(node, textbox, isHeader, true);
                    continue;
                }

                //Modify by Stan 20171204: 把共通的邏輯移出
                var style = GetStyles(node);
                var fontSize = GetFontSize(node);
                var title = node.ParentNode.GetAttributeValue("data-speed-label", "");
                var prefixText = node.ParentNode.GetAttributeValue("data-speed-before", "");
                var postfixText = node.ParentNode.GetAttributeValue("data-speed-after", "");
                var content = $"{prefixText}{node.InnerText.Replace("\n", "").Replace("\r", "")}{postfixText}";
                var paragraph = new Paragraph(_doc);
                textbox.AppendChild(paragraph);
                _builder.MoveTo(paragraph);

                _builder.ParagraphFormat.LineSpacingRule = LineSpacingRule.Exactly;
                _builder.ParagraphFormat.LineSpacing = fontSize;
                _builder.ParagraphFormat.Style.Font.Size = fontSize;
                var titleLength = GetTitleLength(title);
                _builder.ParagraphFormat.LeftIndent = titleLength * fontSize;
                _builder.ParagraphFormat.FirstLineIndent = titleLength * fontSize * -1;
                _builder.Font.Size = fontSize;
                //Add by Stan 20171202: 判斷class有MarginLeftXforXpt才需要Indent
                if (!contentNode.GetAttributeValue("class", "").Contains("MarginLeft"))
                {
                    _builder.ParagraphFormat.LeftIndent = 0;
                    _builder.ParagraphFormat.FirstLineIndent = 0;
                }
                if (style.ContainsKey("margin-bottom"))
                    _builder.ParagraphFormat.SpaceAfter = double.Parse(style["margin-bottom"]);
                if (style.ContainsKey("margin-top"))
                    _builder.ParagraphFormat.SpaceBefore = double.Parse(style["margin-top"]);

                if (node.OriginalName == "#text" && (node.ParentNode.GetAttributeValue("data-speed-id", "") != "" || node.InnerText == "簽"))
                {
                    _builder.InsertHtml($"{title}{EscapeHtml(content)}", true);
                }
                else if (node.OriginalName == "div" && node.GetAttributeValue("data-speed-label", "") != "" ||
                         (node.HasChildNodes && node.FirstChild.OriginalName == "#text" && node.ChildNodes.Count == 1 && node.OriginalName != "label") || //避免孩子是label的情況，否則會重複輸出
                         node.GetAttributeValue("data-speed-id", "") == "PrintAddress")
                {
                    //<div>text</div> (機關地址)
                    //根據目前node讀取title與content
                    title = node.GetAttributeValue("data-speed-label", "");
                    //Modify by Stan 20180612: 補回漏掉的prefix, postfix
                    //pureContent用來計算確切的字數，如果用content的話會把html內容也算進去
                    var pureContent = "";
                    pureContent += prefixText;
                    foreach (var childNode in node.ChildNodes)
                    {
                        if (childNode.Name != "br")
                            pureContent += childNode.InnerText;
                        else
                            pureContent += "<br>";
                    }
                    pureContent += postfixText;
                    //防止多種奇怪的狀況，所以一併替換成換行符號
                    pureContent = pureContent.Replace("\r\n", "").Replace("\n", "");
                    content = $"{prefixText}{node.InnerHtml}{postfixText}".Replace("\r\n", "").Replace("\n", "");
                    // Add by Stan 20171018: 受文者地址處理 
                    var id = node.GetAttributeValue("data-speed-id", "");
                    if (id == "PrintAddress")
                    {
                        //強制設定文字大小為12pt
                        fontSize = 12D;
                        var width = ConvertUtil.MillimeterToPoint(
                            Convert.ToDouble(contentNode.GetAttributeValue("data-speed-width", "0")));
                        var height =
                            ConvertUtil.MillimeterToPoint(
                                Convert.ToDouble(contentNode.GetAttributeValue("data-speed-height", "0")));
                        var contentInLines = pureContent.Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
                        var length = contentInLines.Length;
                        if (width != 0)
                        {
                            //由寬度算出最大一行可容納的字元數量
                            var max = Convert.ToInt32(Math.Floor(width / fontSize));
                            //如果郵遞區號或地址超過該行字元數量的話，就多加一行
                            length += contentInLines.Count(line => line.Length > max);
                        }
                        //用MarginTop來模擬文字置底 (Word的文字方塊中好像沒辦法置底?)
                        //Modify by Stan 20180324: margin如果是負數會拋exception，所以加上防呆。
                        var margin = height - fontSize * length;
                        textbox.TextBox.InternalMarginTop = margin < 0 ? 0 : margin;
                        textbox.Height = height;
                    }
                    // Add by Stan 20171028: 機關地址縮成一行 (CPA ShrinkAddress)
                    else if (_shrinkAddress && id == "Address")
                    {
                        var width = ConvertUtil.MillimeterToPoint(Convert.ToDouble(contentNode.GetAttributeValue("data-speed-width", "0")));
                        if (width != 0)
                        {
                            //由寬度算出最大一行可容納的字元數量，但如果小於5還是容不下的話就不管了
                            while ($"{title}{pureContent}".Length > Convert.ToInt32(Math.Floor(width / fontSize)) && fontSize > 5)
                            {
                                fontSize = fontSize - 0.5;
                            }
                        }
                    }
                    //根據有可能變動的部分再設定一次
                    _builder.Font.Size = fontSize;
                    _builder.ParagraphFormat.LineSpacing = fontSize;
                    if (node.GetAttributeValue("class", "").Contains("MarginLeft"))
                    {
                        titleLength = GetTitleLength(title);
                        _builder.ParagraphFormat.LeftIndent = titleLength * fontSize;
                        _builder.ParagraphFormat.FirstLineIndent = titleLength * fontSize * -1;
                    }

                    _builder.InsertHtml($"{title}{EscapeHtml(content)}", true);
                }
                //內容有表格的狀態
                else if (contentNode.HasChildNodes && contentNode.SelectNodes(".//table")?.Count > 0)
                {
                    //算出裡面最大寬度
                    var maxWidth = 0D;
                    foreach (var tempNode in contentNode.ChildNodes)
                    {
                        var nodeWidth = 0D;
                        style = GetStyles(tempNode);
                        if (style.ContainsKey("width"))
                        {
                            nodeWidth = Convert.ToDouble(style["width"]);
                        }
                        else if (tempNode.GetAttributeValue("data-speed-width", "") != "")
                        {
                            nodeWidth = ConvertUtil.MillimeterToPoint(Convert.ToDouble(tempNode.GetAttributeValue("data-speed-width", "")));
                        }
                        if (nodeWidth > maxWidth)
                            maxWidth = nodeWidth;
                    }
                    textbox.Width = maxWidth;
                    content = contentNode.InnerHtml;
                    _builder.InsertHtml($"{_tableCSS}{EscapeHtml(content)}", true);
                    foreach (Table tableNode in textbox.GetChildNodes(NodeType.Table, true).OfType<Table>())
                    {
                        tableNode.CellSpacing = 0;
                    }
                    break;
                }
                else if (node.OriginalName == "div" && node.ChildNodes.Count > 1 && node.FirstChild.OriginalName != "label")
                {
                    //聯絡資訊/檔號
                    //<div> <div>text</div> ... </div>
                    //一坨Control (FOREST簽)
                    //<div> <div data-speed-id=XXX>...</div> ... </div>

                    //因為內部為多個內容，需要根據每個childNode塞入不同的paragraph，所以先刪除原本預先塞入的paragraph(LastParagraph)
                    textbox.RemoveChild(textbox.LastParagraph);
                    foreach (var nodeChildNode in node.ChildNodes)
                    {
                        if (nodeChildNode.OriginalName == "#text" || nodeChildNode.OriginalName == "#comment" || nodeChildNode.GetAttributeValue("class", "").Contains("noprint"))
                            continue;

                        paragraph = new Paragraph(_doc);
                        textbox.AppendChild(paragraph);
                        _builder.MoveTo(paragraph);

                        style = GetStyles(nodeChildNode);
                        title = nodeChildNode.GetAttributeValue("data-speed-label", "");
                        content = nodeChildNode.InnerText.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
                        fontSize = GetFontSize(node);
                        // Add by Stan 20171203: FOREST簽範本輸出異常處理，加上label與placeHolder判斷
                        if (nodeChildNode.FirstChild?.OriginalName == "label")
                        {
                            content = nodeChildNode.FirstChild.InnerHtml;
                            fontSize = GetFontSize(nodeChildNode.FirstChild);
                        }
                        var placeHolder = nodeChildNode.GetAttributeValue("data-speed-placeholder", "");
                        if (placeHolder != String.Empty && placeHolder == content)
                            continue;

                        _builder.ParagraphFormat.LineSpacingRule = LineSpacingRule.Exactly;
                        _builder.ParagraphFormat.LineSpacing = fontSize;
                        titleLength = GetTitleLength(title);
                        _builder.ParagraphFormat.LeftIndent = titleLength * fontSize;
                        _builder.ParagraphFormat.FirstLineIndent = titleLength * fontSize * -1;
                        if (style.ContainsKey("margin-bottom"))
                            _builder.ParagraphFormat.SpaceAfter = double.Parse(style["margin-bottom"]);
                        if (style.ContainsKey("margin-top"))
                            _builder.ParagraphFormat.SpaceBefore = double.Parse(style["margin-top"]);

                        FontSetting(fontSize);
                        _builder.InsertHtml($"{title}{EscapeHtml(content)}", true);
                    }
                }
                //else if (node.HasChildNodes && node.FirstChild.OriginalName == "div")
                //    InsertFloatItem(node, textbox, isHeader, true);
                else if (contentNode.GetAttributeValue("data-speed-id", "") == "FullName")
                {
                    FontSetting(contentNode);
                    _builder.ParagraphFormat.SpaceAfter = 0;
                    _builder.ParagraphFormat.SpaceBefore = 0;
                    _builder.InsertHtml($"{title}{EscapeHtml(content)}", true);
                    break;
                }
                else if (contentNode.HasChildNodes && contentNode.FirstChild.OriginalName == "label")
                {
                    //修正擬辦方式等noprint class在label上的標籤
                    if (contentNode.FirstChild.GetAttributeValue("class", "").Contains("noprint")) break;
                    _builder.ParagraphFormat.LeftIndent = 0;
                    _builder.ParagraphFormat.FirstLineIndent = 0;
                    content = contentNode.FirstChild.InnerHtml;
                    FontSetting(contentNode);
                    _builder.InsertHtml($"{title}{EscapeHtml(content)}", true);
                    break;
                }
                // Add by Stan 20180515: FOREST範本特殊情況 <div style="position:absolute;"> <div> <label> </div> ... </div>
                else if (node.HasChildNodes && node.FirstChild.OriginalName == "label")
                {
                    if (node.GetAttributeValue("class", "").Contains("noprint")) continue;
                    _builder.ParagraphFormat.LeftIndent = 0;
                    _builder.ParagraphFormat.FirstLineIndent = 0;
                    content = node.FirstChild.InnerHtml;
                    FontSetting(contentNode);
                    _builder.InsertHtml($"{title}{EscapeHtml(content)}", true);
                    continue;
                }
                //支援圖片輸出 <div style="position:absolute;"> <img scr="xxx"/> </div>
                else if (contentNode.HasChildNodes && contentNode.SelectNodes(".//img")?.Count > 0)
                {
                    _builder.ParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
                    content = contentNode.InnerHtml;
                    _builder.InsertHtml($"{EscapeHtml(content)}", true);
                    break;
                }
            }
            _builder.MoveTo(_doc.FirstSection.Body.LastParagraph);
        }

        private double GetTitleLength(string title)
        {
            //計算實際字數(四個半形空白視為一個全形空白)
            return title.Length - title.Count(x => x == ' ') + (title.Count(x => x == ' ') / 4D);
        }

        private void InsertItem(HtmlNode contentNode, bool sameLine = false, bool isCell = false, HtmlNode referenceNode = null)
        {
            var style = GetStyles(contentNode);
            // 如果該Node的Style有 display:none; 則直接跳過不輸出 
            if (style.ContainsKey("display") && style["display"] == "none")
                return;
            // Modify by Stan 20171203: placeHolder處理，如果content一樣就跳過不輸出
            var placeHolder = contentNode.GetAttributeValue("data-speed-placeholder", "");
            if (placeHolder != String.Empty)
            {
                var nodeText = contentNode.InnerText;
                //Dropdown Control處理
                if (contentNode.FirstChild?.OriginalName == "label")
                    nodeText = contentNode.FirstChild.InnerText;

                if (nodeText == placeHolder)
                    return;
            }
            if (style.ContainsKey("text-align"))
                _builder.ParagraphFormat.Alignment = (ParagraphAlignment)Enum.Parse(typeof(ParagraphAlignment), style["text-align"], true);
            if ((style.ContainsKey("text-align-last") && style["text-align-last"] == "justify") ||
                (style.ContainsKey("text-justify") && style["text-justify"] == "distribute-all-lines") ||
                contentNode.GetAttributeValue("class", "").Contains("FormTitleJustify"))
            {
                _builder.ParagraphFormat.Alignment = ParagraphAlignment.Distributed;
            }
            var id = contentNode.GetAttributeValue("data-speed-id", "");
            //2017/07/07 sandy_chiang GSSSPEED-3418 登記桌內部遞送公文要做轉送時失敗
            //Edit by Stan 20171015: 算出正確的RightIndent
            //Edit by Stan 20181203 CTBCODA-345: 如果此node含有table就不用算RightIndent
            if (contentNode.GetAttributeValue("data-speed-width", "") != "" && contentNode.Descendants().All(c => c.Name != "table") && !isCell) //判斷style是否有width屬性
            {
                var width = ConvertUtil.MillimeterToPoint(Convert.ToDouble(contentNode.GetAttributeValue("data-speed-width", "0")));
                _builder.ParagraphFormat.RightIndent = _builder.PageSetup.PageWidth - _builder.PageSetup.LeftMargin - _builder.PageSetup.RightMargin - width;
            }
            //全銜
            if (id == "FullName")
            {
                var fontSize = GetFontSize(contentNode);
                FontSetting(fontSize);
                var textBefore = contentNode.GetAttributeValue("data-speed-before", "");
                var textAfter = contentNode.GetAttributeValue("data-speed-after", "");
                if (style.ContainsKey("margin-top"))
                    _builder.ParagraphFormat.SpaceBefore = double.Parse(style["margin-top"]);
                if (style.ContainsKey("margin-bottom"))
                    _builder.ParagraphFormat.SpaceAfter = double.Parse(style["margin-bottom"]);
                _builder.ParagraphFormat.LineSpacingRule = LineSpacingRule.Exactly;
                _builder.ParagraphFormat.LineSpacing = fontSize;
                _builder.Writeln($"{textBefore}{contentNode.InnerText}{textAfter}");
                _builder.ParagraphFormat.ClearFormatting();
            }
            //署名 or 署名章
            else if (id == "Signature" || id == "SignatureSeal")
            {
                var content = contentNode.InnerHtml;
                FontSetting(contentNode);
                // Edit by Stan 20171029: 還是改回SpaceBefore以符合CSS設定
                if (style.ContainsKey("margin-top"))
                    _builder.ParagraphFormat.SpaceBefore = double.Parse(style["margin-top"]);
                var clearHtml = content.Replace("\n", "<br>");

                if (contentNode.GetAttributeValue("data-speed-signaturemode", "") != "img")
                {
                    //如果不是圖片才用固定行高，否則圖片會被蓋住
                    _builder.ParagraphFormat.LineSpacingRule = LineSpacingRule.Exactly;
                    _builder.ParagraphFormat.LineSpacing = GetFontSize(contentNode);
                }
                else
                {
                    _builder.ParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
                    //Add by Stan 20180201: 署名章圖片中如有div結構會多換行，所以直接抓img輸出就好
                    clearHtml = contentNode.SelectSingleNode("//img")?.OuterHtml;
                }

                _builder.Write("");
                _builder.InsertHtml(EscapeHtml(clearHtml), true);
                // Add by Stan 20171029: 支援同行輸出
                if (!sameLine)
                {
                    _builder.Writeln();
                    _builder.ParagraphFormat.ClearFormatting();
                }
            }
            //段落處理
            else if (id == "paragraph" || id == "tableparagraph")
            {
                var paragraphNodeCollection = contentNode.SelectNodes("./li");
                // Add by Stan 20171028: 如果段落裡面沒有li就略過不管
                if (paragraphNodeCollection == null)
                    return;
                //var counter = 0;
                foreach (var paragraphNode in paragraphNodeCollection)
                {
                    // Add by Stan 20170502: 把段落中的空白與換行去除掉
                    var after = Regex.Replace(paragraphNode.InnerHtml, "\r\n\\s+(?![^<]*</>)", "");
                    // Add by Stan 20171027: 換行符號有可能是 \r\n 也有可能是 \n 所以兩個都取代比較保險
                    after = Regex.Replace(after, "\n\\s+(?![^<]*</>)", "");
                    paragraphNode.InnerHtml = after;
                    if (paragraphNode.Attributes.Count == 0 ||
                        paragraphNode.SelectNodes(".//ul[@data-speed-level]") != null)
                    {
                        InsertParagraphContent(paragraphNode, _listCount, isCell);
                        _listCount++;
                        RemoveLastReturn();
                    }
                    else
                        InsertParagraphTitle(_builder, paragraphNode);
                }
            }
            //職章
            else if (id.Contains("sealContainer") || id == "seal-decision")
            {
                var content = contentNode.InnerHtml.Replace("\r\n", "<br>").Replace("\n", "<br>");
                _builder.ListFormat.RemoveNumbers();
                //表格不分段
                _builder.ParagraphFormat.KeepTogether = true;
                _builder.ParagraphFormat.KeepWithNext = true;
                FontSetting(contentNode);
                _builder.Writeln("");
                _builder.InsertHtml(_tableCSS + _stampCSS + EscapeHtml(content), true);
                //_doc.UpdateTableLayout();
                _builder.ParagraphFormat.ClearFormatting();
            }
            //Add by Stan 20171030: 同行輸出 (署名+署名章) //20171101:職章會符合這個XPATH 先放在職章輸出後
            else if (contentNode.SelectNodes("div[not(./preceding-sibling::label) and not(.//input)]") != null)
            {
                var nodes = contentNode.SelectNodes("div");
                foreach (var htmlNode in nodes)
                {
                    InsertItem(htmlNode, true);
                }
                _builder.Writeln();
                _builder.ParagraphFormat.ClearFormatting();
            }
            //Add by Stan 20171018: 外層表格處理
            else if (contentNode.ChildNodes.Count(x => x.OriginalName == "table") > 0)
            {
                var content = contentNode.InnerHtml;

                //處理td裡面的div，可能有全銜/案由/...
                var tds = contentNode.SelectNodes("//td");
                if (tds != null)
                {
                    foreach (var td in tds)
                    {
                        var elements = td.SelectNodes("div[@data-speed-id]");
                        if (elements != null)
                        {
                            foreach (var element in elements)
                            {
                                var elementId = element.GetAttributeValue("data-speed-id", "");
                                var elementOriginalContent = element.OuterHtml;
                                //字體大小處理，如果只有Attribute沒有Style的話，要補進去
                                var fontSize = element.GetAttributeValue("data-speed-fontsize", "16");
                                var elementStyle = GetStyles(element);
                                if (!elementStyle.ContainsKey("font-size"))
                                {
                                    element.SetAttributeValue("style",
                                        $"font-size:{fontSize}pt;{element.GetAttributeValue("style", "")}");
                                }
                                //全銜處理 TODO:這邊應該可以跟表格內項目符號處理的方式整合?
                                if (elementId == "FullName")
                                {
                                    var textBefore = element.GetAttributeValue("data-speed-before", "");
                                    var textAfter = element.GetAttributeValue("data-speed-after", "");
                                    var text = $"{textBefore}{element.InnerHtml}{textAfter}";
                                    var outer = element.OuterHtml;
                                    content = content.Replace(elementOriginalContent,
                                        outer.Replace(element.InnerHtml, text));
                                }
                                else
                                {
                                    var outer = element.OuterHtml;
                                    content = content.Replace(elementOriginalContent, outer);
                                }
                            }
                        }
                        // Add by Stan 20171103: 表格內項目符號處理 (Modify 20180704: data-speed-id改為tableparagraph)
                        elements = td.SelectNodes(".//ul[@data-speed-id='tableparagraph']") ?? td.SelectNodes(".//ul[@data-speed-id='paragraph']");
                        if (elements == null)
                            continue;
                        foreach (var element in elements)
                        {
                            var currentIndex = _listHtmlData.Count;
                            var target = $"{{@#TABLE_LIST_{currentIndex}#@}}";
                            _listHtmlData.Add(target, element);
                            content = content.Replace(element.OuterHtml, target);
                        }
                    }
                }
                _builder.InsertHtml(_tableCSS + EscapeHtml(content), true);
            }
            else
            {
                // 輸出正本時不必換行，所以先移到上一個換行符號，但如果最後一個段落沒有任何內容就要換行
                //if (id == "OriginalRecipient" && _htmlDoc.DocumentNode.SelectNodes("//ul[@data-speed-id='paragraph']/li[last()]/ul")?.Count > 0)
                //    _builder.MoveTo(_doc.FirstSection.Body.LastParagraph.PreviousSibling);
                // 靠自己算出正確的TOP :(
                if (contentNode.GetAttributeValue("data-speed-top", "") != "")
                {
                    if (style.ContainsKey("top"))
                        _builder.ParagraphFormat.SpaceBefore = Convert.ToDouble(style["top"]);
                    else
                    {
                        var top = ConvertUtil.MillimeterToPoint(Convert.ToDouble(contentNode.GetAttributeValue("data-speed-top", "")));
                        // 選出所有具有 data-speed-top 且 不是文字方塊的 div
                        var relatedNodes = _htmlDoc.DocumentNode.SelectNodes("//div[@class='ContentEditor']/div[@data-speed-top and not(@style[contains(.,'absolute')])]");
                        // 選出自己前一個的node
                        var targetMargin = 0D;
                        var index = -1;
                        for (var i = 0; i < relatedNodes.Count; i++)
                        {
                            var node = relatedNodes[i];
                            if (node.InnerText == contentNode.InnerText)
                            {
                                index = i - 1;
                                break;
                            }
                        }
                        if (index > -1)
                        {
                            var targetNode = relatedNodes[index];
                            targetMargin += GetFontSize(targetNode);
                            var targetStyle = GetStyles(targetNode);
                            if (targetStyle.ContainsKey("margin-top"))
                                targetMargin += Convert.ToDouble(targetStyle["margin-top"]);
                            if (targetStyle.ContainsKey("margin-bottom"))
                                targetMargin += Convert.ToDouble(targetStyle["margin-bottom"]);
                        }
                        // Edit by Stan: SpaceBefore不能是負的，所以要防呆
                        var spaceBefore = top - _builder.PageSetup.TopMargin - targetMargin;
                        _builder.ParagraphFormat.SpaceBefore = spaceBefore < 0 ? 0 : spaceBefore;
                    }

                }
                var title = contentNode.GetAttributeValue("data-speed-label", "");
                var content = contentNode.InnerHtml;
                if (contentNode.HasChildNodes && contentNode.FirstChild.OriginalName == "label" && contentNode.Descendants().Any(c => c.Name == "input"))
                {
                    content = contentNode.FirstChild.InnerHtml;
                }
                var fontSize = GetFontSize(contentNode);
                if (isCell && referenceNode != null)
                {
                    var fontSizeNode = referenceNode.Ancestors()
                                                    .FirstOrDefault(n => n.GetAttributeValue("data-speed-fontsize", "") != "");
                    if (fontSizeNode != null)
                        fontSize = GetFontSize(fontSizeNode);
                }
                //Add by Stan 20180306: 如果CSS有指定line-height就用CSS的設定，預設是用字體大小。
                var lineHeightStyle = style;
                //Add by Stan 20181212: 表格範本情況下，如果contentNode沒有指定行距的話就嘗試從TD抓
                if (!lineHeightStyle.ContainsKey("line-height") &&
                    isCell && referenceNode != null && referenceNode.Name == "td")
                {
                    lineHeightStyle = GetStyles(referenceNode);
                }
                var lineHeight = 1D;
                if (lineHeightStyle.ContainsKey("line-height") && !double.TryParse(lineHeightStyle["line-height"], out lineHeight))
                {
                    lineHeight = 1D;
                }
                _builder.ParagraphFormat.LineSpacingRule = LineSpacingRule.Exactly;
                _builder.ParagraphFormat.LineSpacing = lineHeight * fontSize;

                // Modify by Stan 20180130: data-speed-splitpage=false的話，同段落要在同一頁 (\r: SHIFT+ENTER)。但如果沒設定的話，就視為false
                var splitPage = contentNode.GetAttributeValue("data-speed-splitpage", "");
                if (!string.IsNullOrEmpty(splitPage) && !Convert.ToBoolean(splitPage))
                {
                    _builder.ParagraphFormat.KeepTogether = true;
                    _builder.ParagraphFormat.KeepWithNext = true;
                }
                else
                {
                    _builder.ParagraphFormat.KeepTogether = false;
                    _builder.ParagraphFormat.KeepWithNext = false;
                }

                if (style.ContainsKey("margin-top"))
                    _builder.ParagraphFormat.SpaceBefore = double.Parse(style["margin-top"]);
                if (style.ContainsKey("margin-bottom"))
                    _builder.ParagraphFormat.SpaceAfter = double.Parse(style["margin-bottom"]);
                FontSetting(fontSize);
                _builder.ParagraphFormat.LeftIndent = title.Length * fontSize;
                if (contentNode.GetAttributeValue("data-speed-left", "") != "")
                {
                    if (!style.ContainsKey("left"))
                    {
                        _builder.ParagraphFormat.LeftIndent =
                            ConvertUtil.MillimeterToPoint(
                                Convert.ToDouble(contentNode.GetAttributeValue("data-speed-left", "0"))) - _builder.PageSetup.LeftMargin;
                    }
                    else
                    {
                        _builder.ParagraphFormat.LeftIndent = GetRelativeLeftFromPage(style["left"]);
                    }
                }
                _builder.ParagraphFormat.FirstLineIndent = title.Length * fontSize * -1;

                //Add by Stan 20171202: 判斷class有MarginLeftXforXpt才需要Indent
                if (!contentNode.GetAttributeValue("class", "").Contains("MarginLeft"))
                {
                    //Add by Stan 20171225: 但如果本來就有指定 data-speed-left 就不應該把indent清掉
                    if (contentNode.GetAttributeValue("data-speed-left", "") == "")
                        _builder.ParagraphFormat.LeftIndent = 0;
                    _builder.ParagraphFormat.FirstLineIndent = 0;
                }

                //實際寬度不包含data-speed-label，所以要把RightIndent扣掉label的部分
                if (contentNode.GetAttributeValue("data-speed-width", "") != "")
                    _builder.ParagraphFormat.RightIndent -= _builder.ParagraphFormat.LeftIndent;
                _builder.Write($"{title}");
                var clearHtml = content.Replace("<div>", "<br>");
                _builder.InsertHtml(EscapeHtml(clearHtml), true);
                //Add by Stan 20171019: 如果沒有其他元素要輸出的話，就不用換行跟清格式了
                if (contentNode.NextSibling != null)
                {
                    _builder.Writeln();
                    _builder.ParagraphFormat.ClearFormatting();
                }
            }
        }
        private void InsertParagraphTitle(DocumentBuilder builder, HtmlNode paragraphNode)
        {
            var titleName = paragraphNode.GetAttributeValue("data-speed-paragraph", "");
            // Edit by Stan 20171028: 保留全形空白
            var titleContent = paragraphNode.InnerHtml.Replace("\u3000", "{#fullwidthspace#}").Trim().Replace("{#fullwidthspace#}", "\u3000");
            // Add by Stan 20180129: 便簽段落沒有標題只有項目符號，如果段落內容為空就直接略過不輸出
            if (string.IsNullOrEmpty(titleContent))
                return;
            FontSetting(paragraphNode);
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Justify;
            builder.CellFormat.VerticalAlignment = CellVerticalAlignment.Top;
            // Add by Stan 20170522: 增加段落行高設定
            var style = GetStyles(paragraphNode);
            builder.ParagraphFormat.LineSpacingRule = LineSpacingRule.Multiple;
            builder.ParagraphFormat.LineSpacing = 13.8; // 12 * 1.15 (line-height: 1.5;)
            if (style.ContainsKey("line-height"))
            {
                var lineHeight = Convert.ToDouble(style["line-height"]);
                builder.ParagraphFormat.LineSpacing = 12 * (1 + lineHeight / 10); // 12 * 1.15 (line-height: 1.5;)
            }
            FontSetting(paragraphNode);
            if (style.ContainsKey("margin-top"))
                builder.ParagraphFormat.SpaceBefore = double.Parse(style["margin-top"]);
            if (style.ContainsKey("margin-bottom"))
                builder.ParagraphFormat.SpaceAfter = double.Parse(style["margin-bottom"]);
            builder.ParagraphFormat.LeftIndent = titleName.Length * GetFontSize(paragraphNode);
            builder.ParagraphFormat.FirstLineIndent = titleName.Length * GetFontSize(paragraphNode) * -1;
            //Add by Stan 20171117: 有部分範本會在段落標題輸出表格，這邊統一加上 tableCSS
            builder.InsertHtml($"{_tableCSS}{titleName}{EscapeHtml(titleContent)}", true);
            builder.Writeln();
            builder.ParagraphFormat.ClearFormatting();
        }
        private void InsertParagraphContent(HtmlNode paragraphNode, int listIndex, bool isCell = false)
        {
            _builder.ListFormat.List = _doc.Lists.Add(ListTemplate.NumberDefault);
            ListSetting(listIndex, isCell);
            InsertParagraphContent(paragraphNode, 0, listIndex, isCell);
            _builder.ListFormat.RemoveNumbers();
            _builder.Writeln();
        }
        private void InsertParagraphContent(HtmlNode node, int level, int listIndex, bool isCell = false)
        {
            //印深度
            //加上防呆，因為node有可能是空的 (UL內沒有LI)
            if (node == null)
                return;
            //判斷child是否為ul
            if (node.HasChildNodes && node.FirstChild.OriginalName == "ul")
            {
                InsertParagraphContent(node.ChildNodes[0], 0, listIndex, isCell);
            }
            else if (node.HasChildNodes && node.FirstChild.HasChildNodes && node.FirstChild.FirstChild.Name == "ul")
            {
                var nextLevel = level + 1;
                if (node.FirstChild.FirstChild.GetAttributeValue("data-speed-level", "") != String.Empty)
                    nextLevel = Convert.ToInt32(node.FirstChild.FirstChild.GetAttributeValue("data-speed-level", "")) - 1;
                InsertParagraphContent(node.FirstChild.FirstChild.FirstChild, nextLevel, listIndex, isCell);
            }
            else
            {
                if (node.Attributes["data-speed-level"] == null)
                    _builder.ListFormat.ListLevelNumber = level;
                // Modify by Stan: 空段落防呆
                else if (node.Name.ToUpper().Equals("UL") && !string.IsNullOrEmpty(node.ChildNodes.FirstOrDefault(x => x.NodeType == HtmlNodeType.Element)?.GetAttributeValue("data-speed-itemstyle", "")))
                {
                    var listLevelNumber = node.GetAttributeValue("data-speed-level", 1) - 1;
                    _builder.ListFormat.ListLevelNumber = listLevelNumber;
                }
                else
                {
                    _builder.ListFormat.ListLevelNumber = 8;
                }
                // Add by Stan 20180223: 根據li的itemStyle來決定階層
                // Modify by Stan 20180614: 針對CPA檔管驗證修正，之後須再測試追蹤修訂的其他情況
                if (node.OriginalName == "li" && node.GetAttributeValue("data-speed-itemstyle", "") != "")
                {
                    _builder.ListFormat.ListLevelNumber = GetItemLevel(node.GetAttributeValue("data-speed-itemstyle", "")) - 1;
                }
                else if (node.HasChildNodes && node.FirstChild.OriginalName == "li" && node.FirstChild.GetAttributeValue("data-speed-itemstyle", "") != "")
                {
                    _builder.ListFormat.ListLevelNumber = GetItemLevel(node.FirstChild.GetAttributeValue("data-speed-itemstyle", "")) - 1;
                }
                // Modify by Stan 20180506: li沒有itemstyle的時候，就直接指定為8 (追蹤修訂的情況下會發生)
                else if (node.OriginalName == "li" && node.GetAttributeValue("data-speed-itemstyle", "") == "")
                {
                    _builder.ListFormat.ListLevelNumber = 8;
                    //如果是因為只有old-itemstyle的話，把LIST設定移除
                    if (node.GetAttributeValue("data-speed-old-itemstyle", "") != "")
                    {
                        _builder.ListFormat.List = null;
                        _builder.ParagraphFormat.LeftIndent = 16;
                    }
                    if (node.GetClasses().Count() == 0 && (node.ParentNode.GetAttributeValue("data-speed-level", "") == "" || node.ParentNode.GetAttributeValue("data-speed-level", "") == "1"))
                    {
                        _builder.ListFormat.List = null;
                        _builder.ParagraphFormat.LeftIndent = 16;
                    }
                }
                // 取node的text
                var text = "";
                foreach (var childNode in node.ChildNodes)
                {
                    text += childNode.InnerText;

                    if (childNode.OriginalName == "li" || childNode.OriginalName == "ul")
                        break;
                }
                // 取node的html
                var html = "";
                if (node.OriginalName == "li")
                {
                    html = node.InnerHtml.Replace("&nbsp;", " ").Replace("&nbsp", " ");//html內容不應該出現 &nbsp;
                }
                else
                {
                    foreach (var childNode in node.ChildNodes)
                    {
                        html += childNode.InnerHtml.Replace("&nbsp;", " ").Replace("&nbsp", " ");//html內容不應該出現 &nbsp;

                        if (childNode.OriginalName == "li" || childNode.OriginalName == "ul")
                            break;
                    }
                }

                var fontSize = GetFontSize(node);
                var style = GetStyles(node);
                if (node.HasChildNodes && node.FirstChild.Name != "#text" && node.FirstChild.InnerHtml == html)
                {
                    style = GetStyles(node.FirstChild);
                }
                // Add by Stan 20171212: 支援製作字級功能
                if (style.ContainsKey("font-size"))
                    fontSize = Convert.ToDouble(style["font-size"]);
                // 設定字體大小
                FontSetting(fontSize);
                // Add by Stan 20170522: 增加段落行高設定
                // 20190826 調整成固定行高
                _builder.ParagraphFormat.LineSpacingRule = LineSpacingRule.Exactly;
                _builder.ParagraphFormat.LineSpacing = 25; // 12 * 1.15 (line-height: 1.5;)
                                                           //與後段距離
                                                           //_builder.ParagraphFormat.SpaceAfter = 13.8;
                                                           // 20190924 與後段距離調整為0.5行
                _builder.ParagraphFormat.SpaceAfter = fontSize * 0.5;
                if (style.ContainsKey("line-height"))
                {
                    var lineHeight = Convert.ToDouble(style["line-height"]);
                    _builder.ParagraphFormat.LineSpacingRule = LineSpacingRule.Exactly;
                    _builder.ParagraphFormat.LineSpacing = fontSize * lineHeight; // 12 * 1.15 (line-height: 1.5;)
                }
                // Add by Stan 20170502: 段落預設改為左右對齊
                _builder.ParagraphFormat.Alignment = ParagraphAlignment.Justify;
                if (style.ContainsKey("text-align"))
                    _builder.ParagraphFormat.Alignment = (ParagraphAlignment)Enum.Parse(typeof(ParagraphAlignment), style["text-align"], true);

                if (text == html)
                {
                    // Add by Stan 20171027: 沒文字就不用輸出 (如同製作邏輯)
                    if (text != String.Empty)
                    {
                        //Modify by Stan 20171204: 文字內容不應該有任何換行符號，有也要是<br>
                        _builder.Writeln(text.Replace("\n", "").Replace("\r", ""));
                        _builder.ParagraphFormat.ClearFormatting();
                        ListSetting(listIndex, isCell);
                    }
                }
                else
                {
                    var htmlContent = new HtmlDocument();
                    htmlContent.LoadHtml(html);
                    //var subItem = htmlContent.DocumentNode.SelectNodes("//span[@class='sub']");
                    //var supItem = htmlContent.DocumentNode.SelectNodes("//span[@class='sup']");
                    var tableItem = htmlContent.DocumentNode.SelectNodes("//table");
                    if (tableItem != null)
                    {
                        var insertHtml = "";
                        for (var i = 0; i < tableItem.Count; i++)
                        {
                            var table = tableItem[i];
                            //處理合併寬度問題
                            var colgroupNode = table.SelectSingleNode("colgroup");
                            if (colgroupNode != null)
                            {
                                HtmlNode target = null;
                                for (var x = 0; x < colgroupNode.ChildNodes.Count; x++)
                                {
                                    var current = x;
                                    var targets = table.SelectNodes($"tbody//td[@data-col='{current}']");
                                    var tempTarget = targets?.Select(t => t.GetAttributeValue("colspan", "") != "" ? t : null).FirstOrDefault();
                                    if (tempTarget != null)
                                        target = tempTarget;

                                    var hideNodes = targets?.Select(t => t.GetAttributeValue("style", "").Replace(" ", "").Contains("display:none") ? t : null).ToList();
                                    var hideCount = targets?.Count(t => t.GetAttributeValue("style", "").Replace(" ", "").Contains("display:none"));
                                    if (hideCount == targets?.Count && hideCount != null && targets?.Count != null)
                                    {
                                        var colspan = Convert.ToInt32(targets[0].GetAttributeValue("colspan", "0"));
                                        var targetCol = colgroupNode.ChildNodes[target.GetAttributeValue("data-col", current - 1)];
                                        var targetWidth = Convert.ToDouble(GetStyles(targetCol)["width"]);
                                        var mergedWidth = Convert.ToDouble(GetStyles(colgroupNode.ChildNodes[current])["width"]);

                                        targetCol.SetAttributeValue("style", $"width: {targetWidth + mergedWidth}pt;");
                                        hideNodes.ForEach(n =>
                                        {
                                            if (n == null)
                                                return;
                                            n.ParentNode.RemoveChild(n);
                                        });
                                    }
                                }
                            }

                            //var emptyTds = table.SelectNodes("tbody//td").Select(t=> t.GetAttributeValue("style", "").Replace(" ", "").Contains("width:0") ? t : null).ToList();
                            //emptyTds.ForEach(td =>
                            //{
                            //    if (td == null)
                            //        return;
                            //    var original = td.GetAttributeValue("style", "");
                            //    td.SetAttributeValue("style", $"{original} border-style: hidden !important;");
                            //});

                            var textInParentBefore = "";
                            var textInParentBeforeNode = table.ParentNode.PreviousSibling;
                            while (textInParentBeforeNode != null && textInParentBeforeNode.OriginalName != "div")
                            {
                                //第二個TABLE的話，前面就不需要<br>去撐項目符號了
                                if (i > 0 && textInParentBeforeNode.OriginalName == "br")
                                {
                                    textInParentBeforeNode = textInParentBeforeNode.PreviousSibling;
                                    continue;
                                }

                                textInParentBefore = textInParentBeforeNode.OuterHtml + textInParentBefore;
                                textInParentBeforeNode = textInParentBeforeNode.PreviousSibling;
                            }
                            var textInParentAfter = "";
                            var textInParentAfterNode = table.ParentNode.NextSibling;
                            while (textInParentAfterNode != null && textInParentAfterNode.OriginalName != "div")
                            {
                                //最後一個TABLE，後面需要<br>去撐項目符號
                                if (i != tableItem.Count - 1 && textInParentAfterNode.OriginalName == "br")
                                {
                                    textInParentAfterNode = textInParentAfterNode.NextSibling;
                                    continue;
                                }
                                textInParentAfter += textInParentAfterNode.OuterHtml;
                                textInParentAfterNode = textInParentAfterNode.NextSibling;
                            }
                            //Add by Stan 20171121: 只需要最後有<br>，如果前面有則要移除，否則會多一個換行
                            if (textInParentAfter.StartsWith("<br>") && textInParentAfter.Length > 4)
                                textInParentAfter = textInParentAfter.Substring(4);
                            if (i == 0)
                                _builder.InsertHtml($@"<div>{EscapeHtml(textInParentBefore)}</div>", true);
                            else
                                insertHtml += $@"<div>{EscapeHtml(textInParentBefore)}</div>";
                            var levelIndent = GetLevelIndent(_builder.ListFormat.ListLevelNumber);
                            //table.SetAttributeValue("border", "1");
                            //table.SetAttributeValue("bordercolor", "#000000");
                            //table.SetAttributeValue("style", $"border-collapse:collapse;width:auto; margin-left: {levelIndent}px;");
                            table.SetAttributeValue("style",
                                $"margin-left: {levelIndent}px; width: {Convert.ToDouble(GetContentWidth()) - ConvertUtil.PixelToPoint(levelIndent)}pt;");
                            insertHtml += EscapeHtml(table.OuterHtml);
                            if (textInParentAfter != "")
                            {
                                insertHtml += $@"<div style=""margin-left: {levelIndent}px;"">{EscapeHtml(textInParentAfter)}</div>";
                            }
                            //_builder.ListFormat.RemoveNumbers();
                            //_builder.Write("");
                            //_builder.InsertHtml(
                            //    _tableCSS + EscapeHtml(table.OuterHtml) +
                            //    $@"<div style=""margin-left: {levelIndent}px;"">{EscapeHtml(textInParentAfter)}</div>",
                            //    true);
                            //_builder.InsertHtml(textInParentAfter, true);
                            //_builder.Writeln();
                        }
                        _builder.ListFormat.RemoveNumbers();
                        _builder.InsertHtml(_tableCSS + insertHtml, true);
                        ListSetting(listIndex, isCell);
                    }//替換上下標 span -> sup or sub
                     //else if (subItem != null)
                     //{
                     //    //為下標物件
                     //    var clearHtml = html.Replace("<span class=\"sub\">", "<sub>").Replace("</span>", "</sub>");
                     //    _builder.Write("");
                     //    _builder.InsertHtml(clearHtml, true);
                     //    _builder.Writeln();
                     //    ListSetting(listIndex, isCell);
                     //}
                     //else if (supItem != null)
                     //{
                     //    //為上標物件
                     //    var clearHtml = html.Replace("<span class=\"sup\">", "<sup>").Replace("</span>", "</sup>");
                     //    _builder.Write("");
                     //    _builder.InsertHtml(clearHtml, true);
                     //    _builder.Writeln();
                     //    ListSetting(listIndex, isCell);
                     //}
                    else
                    {
                        _builder.Write("");
                        // Edit by Stan 20170509: 強制移除每個<li>最後的 <br>，這樣就可以強制換行(不會有項目符號跑掉的問題) 
                        var clearHtml = EscapeHtml(html);
                        var lastIndex = clearHtml.LastIndexOf("<br>", StringComparison.Ordinal);
                        if (lastIndex != -1 && lastIndex + 4 == clearHtml.Length)
                            clearHtml = clearHtml.Remove(lastIndex);
                        else
                        {
                            // Add by Stan 20180224: 追蹤修訂有可能會把br用span包起來，會造成多換行的問題，需一併移除
                            lastIndex = clearHtml.LastIndexOf("<span><br></span>", StringComparison.Ordinal);
                            if (lastIndex != -1 && lastIndex + 17 == clearHtml.Length)
                                clearHtml = clearHtml.Remove(lastIndex);
                        }
                        _builder.InsertHtml(clearHtml, true);
                        _builder.Writeln();
                        // Add by Stan 20180201: 項目符號有時候會跑版，每次都需要清除格式設定 (同text)
                        _builder.ParagraphFormat.ClearFormatting();
                        ListSetting(listIndex, isCell);
                    }
                }
            }

            //印廣度
            if (node.FirstChild != null && node.FirstChild.NextSibling != null)
            {
                for (var i = 1; i < node.ChildNodes.Count; i++)
                {
                    var childNode = node.ChildNodes[i];
                    if (childNode.OriginalName != "li" && childNode.OriginalName != "ul")
                        continue;
                    var levelNumber =
                        Convert.ToInt32(node.GetAttributeValue("data-speed-level", (level + 1).ToString())) - 1;
                    InsertParagraphContent(childNode, levelNumber, listIndex, isCell);
                    return;
                }
            }
            if (node.NextSibling != null && node.NextSibling.OriginalName == "li" && node.NextSibling.GetAttributeValue("data-speed-paragraph", "HAVENOVALUE") == "HAVENOVALUE")
            {
                var levelNumber =
                    Convert.ToInt32(node.GetAttributeValue("data-speed-level", (level + 1).ToString())) - 1;
                InsertParagraphContent(node.NextSibling, levelNumber, listIndex, isCell);
            }
        }
        private void InsertBarCode(XmlNode node, Dictionary<string, string> setting)
        {
            if (node == null)
                return;

            var position = setting["align"];
            var left = 0;
            switch (position)
            {
                case "right":
                    left = 300;
                    break;
                case "left":
                    left = -10;
                    break;
            }
            if (left == 0)
            {
                setting["align"] = "left";
                InsertBarCode(node, setting);
                setting["align"] = "right";
                InsertBarCode(node, setting);
                setting["align"] = "both";
                return;
            }

            //TOP
            var codeText = node.Attributes["binding"].Value ?? "";
            if (string.IsNullOrEmpty(codeText))
                return;

            foreach (XmlNode content in node.SelectNodes("./top/content"))
            {
                var set = ParseBarCodeSetting(content);
                string display = set.ContainsKey("binding") && set["binding"] != "" ? set["binding"] : set["display"];
                var img = _builder.InsertImage(GetBarCode(codeText, 12, display, Convert.ToInt32(set["fontsize"]), GetDirection(set["align"], true)));
                img.WrapType = WrapType.None;
                img.BehindText = true;
                img.Left = left;
                img.Top = -10;
                img.Width = 120;
                img.Height = 50;
            }
            //BTM
            foreach (XmlNode content in node.SelectNodes("./bottom/content"))
            {
                var set = ParseBarCodeSetting(content);
                string display = set.ContainsKey("binding") ? set["binding"] : set["display"];
                var img = _builder.InsertImage(GetBarCode(codeText, 12, display, Convert.ToInt32(set["fontsize"]), GetDirection(set["align"], false)));
                img.WrapType = WrapType.None;
                img.BehindText = true;
                img.Left = left;
                img.Top = -10;
                img.Width = 120;
                img.Height = 50;
            }
        }
        private byte GetDirection(string align, bool isTop)
        {
            if (isTop)
            {
                switch (align)
                {
                    case "left":
                        return 1;
                    case "center":
                        return 2;
                    case "right":
                        return 3;
                }
            }
            else
            {
                switch (align)
                {
                    case "left":
                        return 4;
                    case "center":
                        return 0;
                    case "right":
                        return 5;
                }
            }
            return 0;
        }

        /// <summary>
        /// 設定條碼Style
        /// </summary>
        /// <param name="barCodeGenerator"></param>
        /// <param name="code"></param>
        /// <param name="codeSize"></param>
        private void setBarcodeStyle(BarcodeGenerator barCodeGenerator, string code, int codeSize)
        {
            barCodeGenerator.Parameters.Resolution = 250;
            barCodeGenerator.Parameters.BackColor = Color.Transparent;
            BarcodeParameters barcode = barCodeGenerator.Parameters.Barcode;
            barcode.Padding.Top.Pixels = 0F;
            barcode.Padding.Bottom.Pixels = 0F;
            barcode.Padding.Left.Pixels = 1F;
            barcode.Padding.Right.Pixels = 1F;
            barcode.WideNarrowRatio = 1.5F;
            barcode.CodeTextParameters.Alignment = TextAlignment.Center;
            barcode.CodeTextParameters.Font.FamilyName = "Times New Roman";
            barcode.CodeTextParameters.Font.Size.Pixels = codeSize;
            barCodeGenerator.CodeText = code;
            barcode.BarHeight.Pixels = 16F;
        }

        /// <summary>
        /// 設定條碼文字style
        /// </summary>
        /// <param name="barCodeGenerator"></param>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <param name="textAlignment"></param>
        /// <param name="codeLocation"></param>
        /// <param name="color"></param>
        private void setBarcodeTextStyle(BarcodeGenerator barCodeGenerator, string text, int fontSize, TextAlignment textAlignment, CodeLocation codeLocation, Color color)
        {
            barCodeGenerator.Parameters.CaptionBelow.Text = text;
            barCodeGenerator.Parameters.CaptionBelow.Alignment = textAlignment;
            barCodeGenerator.Parameters.CaptionBelow.Visible = true;
            barCodeGenerator.Parameters.CaptionBelow.Font.FamilyName = "Time New Roman";
            barCodeGenerator.Parameters.CaptionBelow.Font.Size.Pixels = fontSize * 1.5F;
            barCodeGenerator.Parameters.CaptionBelow.Font.Style = FontStyle.Regular;
            barCodeGenerator.Parameters.CaptionBelow.TextColor = Color.Transparent;
            barCodeGenerator.Parameters.Barcode.CodeTextParameters.Location = codeLocation;
            barCodeGenerator.Parameters.Barcode.BarColor = color;
        }

        private byte[] GetBarCode(string code, int codeSize, string text, int textSize, byte direction)
        {
            BarcodeGenerator barCodeGenerator = new BarcodeGenerator(EncodeTypes.Code39Standard);
            setBarcodeStyle(barCodeGenerator, code, codeSize);

            switch (direction)
            {
                case 0:
                    //Add by Stan 20171015: 如果是空值，寬度會跑掉，需要給空白
                    if (string.IsNullOrEmpty(text))
                        text = " ";
                    setBarcodeTextStyle(barCodeGenerator, text, codeSize, TextAlignment.Center, CodeLocation.Above, Color.Black);
                    break;
                case 1:
                    setBarcodeTextStyle(barCodeGenerator, text, textSize, TextAlignment.Left, CodeLocation.Below, Color.Transparent);
                    break;
                case 2:
                    setBarcodeTextStyle(barCodeGenerator, text, textSize, TextAlignment.Center, CodeLocation.Below, Color.Transparent);
                    break;
                case 3:
                    setBarcodeTextStyle(barCodeGenerator, text, textSize, TextAlignment.Right, CodeLocation.Below, Color.Transparent);
                    break;
                case 4:
                    setBarcodeTextStyle(barCodeGenerator, text, textSize, TextAlignment.Left, CodeLocation.Above, Color.Transparent);
                    break;
                case 5:
                    setBarcodeTextStyle(barCodeGenerator, text, textSize, TextAlignment.Right, CodeLocation.Above, Color.Transparent);
                    break;
            }
            MemoryStream ms = new MemoryStream();
            Bitmap bitmap = barCodeGenerator.GenerateBarCodeImage();
            bitmap.SetResolution(250, 250);
            bitmap.Save(ms, ImageFormat.Bmp);

            return ms.ToArray();
        }
        private void InsertGutter(double gutterLineSpacing)
        {
            if (_gutterImage == null)
                return;
            var img = _builder.InsertImage(_gutterImage);
            img.WrapType = WrapType.None;
            img.BehindText = true;
            img.Left = ConvertUtil.MillimeterToPoint(gutterLineSpacing);
            img.RelativeHorizontalPosition = RelativeHorizontalPosition.Page;
            img.RelativeVerticalPosition = RelativeVerticalPosition.Page;
            img.VerticalAlignment = VerticalAlignment.Center;
        }
        // Edit by Stan 20170502: Header需要特別把左右邊界加回去，所以增加參數來判斷是否要加
        private double GetRelativeLeftFromPage(string value, bool fromHeader = false)
        {
            var pageSetup = _doc.FirstSection.PageSetup;
            var pageWidth = pageSetup.PageWidth - pageSetup.LeftMargin - pageSetup.RightMargin;
            // Header左右邊界為0，所以實際頁面寬度要再加上左右邊界
            if (fromHeader)
                pageWidth += pageSetup.LeftMargin + pageSetup.RightMargin;
            var percent = double.Parse(value.Trim('%')) / 100;
            return pageWidth * percent;
        }
        private Dictionary<string, string> GetStyles(HtmlNode node, DocumentBuilder builder = null)
        {
            if (builder == null)
                builder = _builder;
            // rem換算是用px的
            // FontSize是PT 換成PX才能算REM
            var fontSize = ConvertUtil.PointToPixel(GetFontSize(node));
            var style = node.GetAttributeValue("style", "");
            var dictionary = new Dictionary<string, string>();
            foreach (var item in style.Split(';').Where(s => s != String.Empty))
            {
                var temp = item.Split(':');
                if (temp.Length != 2)
                    continue;
                //取出屬性名稱
                var name = temp[0].Trim().ToLower();
                //取出屬性的值(如果陣列長度為1代表是單值)
                var data = temp[1].Split(' ').Where(d => d != String.Empty).ToList();
                //Modify by Stan 20180315: CSS多值屬性處理(margin, transform-origin...)
                if (data.Count > 1)
                {
                    //針對margin取出 margin-top 與 margin-bottom
                    if (name == "margin")
                    {
                        switch (data.Count)
                        {
                            case 1:
                            case 2:
                                dictionary.Add($"{name}-top", GetStyleValue(builder, fontSize, name, data[0]));
                                dictionary.Add($"{name}-bottom", GetStyleValue(builder, fontSize, name, data[0]));
                                break;
                            case 3:
                            case 4:
                                dictionary.Add($"{name}-top", GetStyleValue(builder, fontSize, name, data[0]));
                                dictionary.Add($"{name}-bottom", GetStyleValue(builder, fontSize, name, data[2]));
                                break;
                        }
                    }
                    //其他屬性先不處理
                    //transform-origin: left center 0px; (這種的先跳過，不需要用到)
                    continue;
                }
                //單值不需要額外處理
                dictionary.Add(name, GetStyleValue(builder, fontSize, name, data.FirstOrDefault()));
            }
            // Add by Stan 20171030: SpaceBefore(margin-top) / SpaceAfter(margin-bottom) 如果是負數，直接替換成0
            if (dictionary.ContainsKey("margin-top") && dictionary["margin-top"].Contains("-"))
                dictionary["margin-top"] = "0";
            if (dictionary.ContainsKey("margin-bottom") && dictionary["margin-bottom"].Contains("-"))
                dictionary["margin-bottom"] = "0";

            return dictionary;
        }

        private string GetStyleValue(DocumentBuilder builder, double fontSize, string name, string value)
        {
            //Add by Stan 20180316: 當margin的值為auto的時候，直接回傳0 (Aspose目前預設值是0)
            if (name.Contains("margin") && value.Trim().ToLower() == "auto")
                return "0";
            //Add by Stan 20180129: 行距判斷，有可能是px也有可能是比例，px需要換算回比例
            if (name == "line-height")
            {
                if (value.Contains("px"))
                    value = (fontSize / ConvertUtil.PixelToPoint(
                                   Convert.ToDouble(value.Remove(value.IndexOf("px", StringComparison.Ordinal)))))
                        .ToString();
                return value.Trim();
            }

            if (value.Contains("px"))
                value = ConvertUtil
                    .PixelToPoint(Convert.ToDouble(value.Remove(value.IndexOf("px", StringComparison.Ordinal)))).ToString();
            if (value.Contains("rem"))
                value = ConvertUtil
                    .PixelToPoint(Convert.ToDouble(value.Remove(value.IndexOf("rem", StringComparison.Ordinal))) * fontSize)
                    .ToString();
            if (value.Contains("em"))
                value = ConvertUtil
                    .PixelToPoint(Convert.ToDouble(value.Remove(value.IndexOf("em", StringComparison.Ordinal))) * 16)
                    .ToString();
            if (value.Contains("mm"))
                value = ConvertUtil
                    .MillimeterToPoint(Convert.ToDouble(value.Remove(value.IndexOf("mm", StringComparison.Ordinal))))
                    .ToString();
            if (value.Contains("pt"))
                value = Convert.ToDouble(value.Remove(value.IndexOf("pt", StringComparison.Ordinal))).ToString();
            if (value.Contains("%") && name != "left")
            {
                var pageWidth = builder.CurrentSection.PageSetup.PageWidth -
                                builder.CurrentSection.PageSetup.LeftMargin -
                                builder.CurrentSection.PageSetup.RightMargin;
                value = (Convert.ToDouble(value.Remove(value.IndexOf("%", StringComparison.Ordinal))) / 100 * pageWidth)
                    .ToString();
            }

            return value.Trim();
        }

        private Dictionary<string, string> GetStyles(string css)
        {
            //.ocseal{font-size:20px;font-weight:700;border:2px solid red;color:red;width:60px;height:30px;position:absolute;top:20px;left:20px;text-align:center}
            var style = css.Replace(".ocseal{", "").Replace("}", "");
            var dictionary = new Dictionary<string, string>();
            foreach (var item in style.Split(';'))
            {
                if (item != String.Empty)
                {
                    var temp = item.Split(':');
                    if (temp.Length != 2)
                        continue;
                    if (temp[1].Contains("px") && !temp[1].Contains(" "))
                        temp[1] = ConvertUtil.PixelToPoint(Convert.ToDouble(temp[1].Remove(temp[1].IndexOf("px", StringComparison.Ordinal)))).ToString();
                    //if (temp[1].Contains("rem"))
                    //    temp[1] = ConvertUtil.PixelToPoint(Convert.ToDouble(temp[1].Remove(temp[1].IndexOf("rem", StringComparison.Ordinal))) * fontSize).ToString();
                    //if (temp[1].Contains("em"))
                    //    temp[1] = ConvertUtil.PixelToPoint(Convert.ToDouble(temp[1].Remove(temp[1].IndexOf("em", StringComparison.Ordinal))) * 12).ToString();
                    dictionary.Add(temp[0].Trim(), temp[1].Trim());
                }
            }
            return dictionary;
        }
        private bool IsAbsolute(HtmlNode contentNode)
        {
            var style = GetStyles(contentNode);
            // Edit by Stan 20170502: 改為只有一個Style也放行，少的參數從Attribute抓就好
            return style.ContainsKey("position") && style["position"] == "absolute" && style.Count > 0;
        }

        private void ListSetting(int index, bool isCell = false)
        {
            var list = _doc.Lists[index];
            for (var i = 0; i < 9; i++)
            {
                list.ListLevels[i].TrailingCharacter = ListTrailingCharacter.Nothing;
                list.ListLevels[i].TabPosition = 0;
                list.ListLevels[i].Font.NameFarEast = DefaultFarEastFont;
                list.ListLevels[i].Font.NameAscii = DefaultAsciiFont;
            }

            list.ListLevels[0].NumberFormat = "\x0000、";
            list.ListLevels[0].NumberStyle = saveFormat == SaveFormat.Odt ? NumberStyle.KanjiDigit : NumberStyle.TradChinNum3;
            list.ListLevels[1].NumberFormat = "(\x0001)";
            list.ListLevels[1].NumberStyle = saveFormat == SaveFormat.Odt ? NumberStyle.KanjiDigit : NumberStyle.TradChinNum3;
            list.ListLevels[2].NumberFormat = "\x0002、";
            list.ListLevels[2].NumberStyle = NumberStyle.ArabicFullWidth;
            list.ListLevels[3].NumberFormat = "(\x0003)";
            list.ListLevels[3].NumberStyle = NumberStyle.ArabicFullWidth;
            list.ListLevels[4].NumberFormat = "\x0004、";
            list.ListLevels[4].NumberStyle = NumberStyle.Zodiac1;
            list.ListLevels[5].NumberFormat = "(\x0005)";
            list.ListLevels[5].NumberStyle = NumberStyle.Zodiac1;
            list.ListLevels[6].NumberFormat = "\x0006、";
            list.ListLevels[6].NumberStyle = NumberStyle.Zodiac2;
            list.ListLevels[7].NumberFormat = "(\x0007)";
            list.ListLevels[7].NumberStyle = NumberStyle.Zodiac2;
            list.ListLevels[8].NumberFormat = "";
            list.ListLevels[8].NumberStyle = NumberStyle.Zodiac2;

            list.ListLevels[0].NumberPosition = 16;
            list.ListLevels[0].TextPosition = 48;
            list.ListLevels[1].NumberPosition = 38;
            list.ListLevels[1].TextPosition = 66;
            list.ListLevels[2].NumberPosition = 85;
            list.ListLevels[2].TextPosition = 85;
            list.ListLevels[3].NumberPosition = 75;
            list.ListLevels[3].TextPosition = 102;
            list.ListLevels[4].NumberPosition = 93;
            list.ListLevels[4].TextPosition = 125;
            list.ListLevels[5].NumberPosition = 142;
            list.ListLevels[5].TextPosition = 142;
            list.ListLevels[6].NumberPosition = 134;
            list.ListLevels[6].TextPosition = 166;
            list.ListLevels[7].NumberPosition = 156;
            list.ListLevels[7].TextPosition = 183;
            list.ListLevels[8].NumberPosition = 48;
            list.ListLevels[8].TextPosition = 48;
            _builder.ListFormat.List = list;
        }
        private int GetLevelIndent(int level)
        {
            switch (level)
            {
                case 0:
                    return 20;
                case 1:
                    return 80;
                case 2:
                    return 110;
                case 3:
                    return 133;
                case 4:
                    return 164;
                case 5:
                    return 186;
                case 6:
                    return 217;
                case 7:
                    return 240;
                default:
                    return 60;
            }
        }
        private void FontSetting(double size)
        {
            _builder.Font.Color = Color.Black;
            _builder.Font.Size = size;
        }
        private void FontSetting(HtmlNode node)
        {
            _builder.Font.Color = Color.Black;
            _builder.Font.Size = GetFontSize(node);
        }
        private double GetFontSize(HtmlNode node)
        {
            var current = node.GetAttributeValue("data-speed-fontsize", "");
            if (current == String.Empty)
                current = node.ParentNode.GetAttributeValue("data-speed-fontsize", DefaultFontSize.ToString(CultureInfo.CurrentCulture));
            return double.Parse(current);
        }
        private Dictionary<string, string> ParseBarCodeSetting(XmlNode node)
        {
            var result = new Dictionary<string, string>();
            if (node != null && node.Attributes != null)
                foreach (XmlAttribute nodeAttribute in node.Attributes)
                    result.Add(nodeAttribute.Name, nodeAttribute.Value);
            return result;
        }

        private string EscapeHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return String.Empty;

            var origin = new HtmlDocument();
            origin.LoadHtml(html);
            var delNodes = origin.DocumentNode.SelectNodes("//del");
            if (delNodes != null)
            {
                foreach (var delNode in delNodes)
                {
                    delNode.Name = "span";
                    delNode.SetAttributeValue("style",
                        $"text-decoration: line-through;{delNode.GetAttributeValue("style", "")}");
                }
            }
            var insNodes = origin.DocumentNode.SelectNodes("//ins");
            if (insNodes != null)
            {
                foreach (var insNode in insNodes)
                    insNode.Name = "span";
            }

            var subNodes = origin.DocumentNode.SelectNodes("//span[@class='sub']");
            if (subNodes != null)
            {
                foreach (var subNode in subNodes)
                {
                    subNode.Name = "sub";
                    subNode.SetAttributeValue("class", "");
                }
            }

            var supNodes = origin.DocumentNode.SelectNodes("//span[@class='sup']");
            if (supNodes != null)
            {
                foreach (var supNode in supNodes)
                {
                    supNode.Name = "sup";
                    supNode.SetAttributeValue("class", "");
                }
            }

            return origin.DocumentNode.OuterHtml;
        }

        //IEnumerable<string> ChunkString(string str, int maxChunkSize)
        //{
        //    for (var i = 0; i < str.Length; i += maxChunkSize)
        //        yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        //}

        private Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static string GetClearHtml(string se)
        {
            //var input = new MemoryStream(Encoding.UTF8.GetBytes(se));
            //var tidyDoc = TidyManaged.Document.FromStream(input);
            //tidyDoc.OutputBodyOnly = AutoBool.Yes;
            //tidyDoc.CharacterEncoding = EncodingType.Utf8;
            //tidyDoc.InputCharacterEncoding = EncodingType.Utf8;
            //tidyDoc.OutputCharacterEncoding = EncodingType.Utf8;
            //tidyDoc.RemoveComments = true;
            //tidyDoc.ForceOutput = true;
            //tidyDoc.LineBreakBeforeBR = false;
            //tidyDoc.DropEmptyElements = false;
            //tidyDoc.CleanAndRepair();
            //se = tidyDoc.Save();
            var doc = new HtmlDocument { OptionFixNestedTags = true };
            HtmlNodeCollection targetNodes;
            HtmlNodeCollection paragraphNodes;
            //1. 先去除tag間所有的換行跟空白
            se = Regex.Replace(se, "\r\n\\s+(?![^<]*</>)", "");
            se = Regex.Replace(se, "\n\\s+(?![^<]*</>)", "");
            //2. 再用HAP補齊LI
            doc.LoadHtml(se);
            //補齊後去除所有註解
            doc.DocumentNode.Descendants()
                .Where(n => n.NodeType == HtmlNodeType.Comment)
                .ToList()
                .ForEach(n => n.Remove());
            se = doc.DocumentNode.OuterHtml;
            //3. 每個 </ul> 前面都應該要有 </li> ，沒有就補齊
            var elements = se.Split(new[] { "</ul>" }, StringSplitOptions.None);
            for (var index = 0; index < elements.Length - 1; index++)
            {
                if (!elements[index].EndsWith("</li>"))
                    elements[index] += "</li>";
            }
            se = string.Join("</ul>", elements);
            //Edit by Stan 20171107: 
            //4&5的處理統一改成while並加入終止條件防止無窮迴圈，執行修補次數最高100次，超過次數會丟Exception
            //支援多個ul[@data-speed-id='paragraph']修補
            var maxCount = 100;
            //4. 除了ul[data-speed-id=paragraph]之外，其他<ul>前應該要是<li>，</ul>後應該要是</li>
            var loopCount = 0;
            bool changed;
            do
            {
                //迴圈初始化
                changed = false;
                loopCount++;
                //HAP修補結構後取出最新SE
                doc.LoadHtml(se);
                se = doc.DocumentNode.OuterHtml;
                //抓出所有的paragraph，沒有就中斷while
                paragraphNodes = doc.DocumentNode.SelectNodes("//ul[@data-speed-id='paragraph']");
                if (paragraphNodes == null)
                    break;
                foreach (var paragraphNode in paragraphNodes)
                {
                    //抓出此paragraph內的所有ul
                    targetNodes = paragraphNode.SelectNodes(".//ul");
                    if (targetNodes == null)
                        continue;

                    foreach (var targetNode in targetNodes)
                    {
                        //ul的爸爸是不是li，不是就需要修補
                        if (targetNode.ParentNode.OriginalName == "li")
                            continue;
                        var outer = targetNode.OuterHtml;
                        var fix = $"<li>{outer}</li>";
                        se = se.Replace(outer, fix);
                        changed = true;
                        break;
                    }
                    //修補後需要重新載入SE(再次透過HAP修補)，所以直接跳出迴圈
                    if (changed)
                        break;
                }
                //如果迴圈次數已達上限，就丟Exception，反正匯出結果也會是錯的
                if (loopCount >= maxCount)
                    throw new Exception("嘗試修補<ul>結構失敗，迴圈次數達上限。");
            } while (changed);

            //5. 最後檢查 <li> 之中是否包含別的 <li> ，如果有就代表有錯
            //counter歸零
            loopCount = 0;
            do
            {
                //迴圈初始化
                changed = false;
                loopCount++;
                //HAP修補結構後取出最新SE
                doc.LoadHtml(se);
                se = doc.DocumentNode.OuterHtml;
                //抓出所有的paragraph，沒有就中斷while
                paragraphNodes = doc.DocumentNode.SelectNodes("//ul[@data-speed-id='paragraph']");
                if (paragraphNodes == null)
                    break;
                foreach (var paragraphNode in paragraphNodes)
                {
                    //抓出此paragraph內的所有包含li的li (代表前面修補失敗)
                    targetNodes = paragraphNode.SelectNodes(".//li[li]");
                    if (targetNodes == null)
                        continue;

                    foreach (var selectNode in targetNodes)
                    {
                        var parentOuter = selectNode.OuterHtml;
                        var outer = selectNode.ChildNodes?.First(n => n.Name == "li")?.OuterHtml ?? "";
                        var pervious = selectNode.ChildNodes?.First(x => x.Name == "li")?.PreviousSibling?.OuterHtml ?? "";
                        var temp = parentOuter.Replace(outer, "{#@ORIGIN@#}");
                        if (temp.EndsWith("</li>"))
                            temp = temp.Remove(temp.LastIndexOf("</li>", StringComparison.Ordinal));
                        temp = temp.Replace(pervious, $"{pervious}</li>");
                        if (se.IndexOf(parentOuter, StringComparison.Ordinal) == -1 && parentOuter.EndsWith("</li>"))
                        {
                            parentOuter = parentOuter.Remove(parentOuter.Length - 5);
                        }
                        se = se.Replace(parentOuter, temp.Replace("{#@ORIGIN@#}", outer));
                        changed = true;
                        if (changed)
                            break;
                    }
                    //修補後需要重新載入SE(再次透過HAP修補)，所以直接跳出迴圈
                    if (changed)
                        break;
                }
                //如果迴圈次數已達上限，就丟Exception，反正匯出結果也會是錯的
                if (loopCount >= maxCount)
                    throw new Exception("嘗試修補<li>結構失敗，迴圈次數達上限。");
            } while (changed);
            //6. 最後檢查 <li data-speed-paragraph> 是否都在最外層
            //counter歸零
            loopCount = 0;
            //將上次調整過的node的outerHtml存起來，供下次調整時找index用
            var previousTargetHtml = "";
            do
            {
                //迴圈初始化
                changed = false;
                loopCount++;
                //HAP修補結構後取出最新SE
                doc.LoadHtml(se);
                se = doc.DocumentNode.OuterHtml;
                //抓出所有的paragraph，沒有就中斷while
                paragraphNodes = doc.DocumentNode.SelectNodes("//ul[@data-speed-id='paragraph']");
                if (paragraphNodes == null)
                    break;
                foreach (var paragraphNode in paragraphNodes)
                {
                    //抓出所有段落標題 (主旨、說明、擬辦...)
                    targetNodes = paragraphNode.SelectNodes(".//li[@data-speed-paragraph]");
                    if (targetNodes == null)
                        continue;

                    foreach (var selectNode in targetNodes)
                    {
                        //是否有段落內容(公文九階)
                        var withList = false;
                        var list = selectNode.NextSibling;
                        if (list != null)
                        {
                            var className = list.GetAttributeValue("class", "");
                            //製作出來的SE，段落內容的有兩種可能： <li> 或 <li class="item-x">，且小孩一定是<ul>
                            if ((!list.HasAttributes || className.Contains("item-")) && list.FirstChild?.Name == "ul")
                            {
                                withList = true;
                            }
                        }
                        var counter = 0;
                        //取出最外層的LI Node，且它的爸爸必須是 ul[data-speed-id=paragraph]
                        var parentLiNode = selectNode.ParentNode;
                        if (parentLiNode == null || parentLiNode.Equals(paragraphNode))
                            continue;
                        while (parentLiNode?.Name != "li" || parentLiNode.ParentNode?.Equals(paragraphNode) != true)
                        {
                            parentLiNode = parentLiNode?.ParentNode;
                            if (++counter > 50)
                            {
                                parentLiNode = null;
                                break;
                            }
                        }
                        //LI Node不為null就代表有找到，必須在指定位置插入
                        if (parentLiNode != null)
                        {
                            //找出要插入的點
                            var index = paragraphNode.ChildNodes.IndexOf(parentLiNode);
                            //如果之前已有調整過的紀錄，就直接插在調整後的node的後面
                            if (!string.IsNullOrEmpty(previousTargetHtml))
                            {
                                index = paragraphNode.ChildNodes.IndexOf(
                                    paragraphNode.ChildNodes.FirstOrDefault(n => n.OuterHtml == previousTargetHtml));
                            }
                            //新增自己
                            paragraphNode.ChildNodes.Insert(index + 1, HtmlNode.CreateNode(selectNode.OuterHtml));
                            if (withList)
                            {
                                paragraphNode.ChildNodes.Insert(index + 2, HtmlNode.CreateNode(list.OuterHtml));
                            }

                        }
                        //LI Node為Null就直接在paragraphNode最後插入
                        else
                        {
                            //直接在最後新增自己
                            paragraphNode.AppendChild(HtmlNode.CreateNode(selectNode.OuterHtml));
                            if (withList)
                                paragraphNode.AppendChild(HtmlNode.CreateNode(list.OuterHtml));
                        }
                        //將調整的node的outerHtml存起來，供下次找index用
                        previousTargetHtml = selectNode.OuterHtml;
                        //刪掉自己
                        selectNode.ParentNode.RemoveChild(selectNode);
                        if (withList)
                        {
                            previousTargetHtml = list.OuterHtml;
                            list.ParentNode.RemoveChild(list);
                        }

                        changed = true;
                        break;
                    }
                    if (changed)
                    {
                        //取出更新後的HTML
                        se = doc.DocumentNode.OuterHtml;
                        break;
                    }
                }
                // 如果迴圈次數已達上限，就丟Exception，反正匯出結果也會是錯的
                if (loopCount >= maxCount)
                    throw new Exception(@"嘗試修補<li data-speed-paragraph=""xxx"">結構失敗，迴圈次數達上限。");
            } while (changed);
            //7. 去除多餘的空段落(防止多換行)
            se = doc.DocumentNode.OuterHtml;
            doc.LoadHtml(se);
            paragraphNodes = doc.DocumentNode.SelectNodes("//ul[@data-speed-id='paragraph']");
            if (paragraphNodes != null)
            {
                foreach (var paragraphNode in paragraphNodes)
                {
                    //取出空段落(ul內沒有li)，並刪除
                    var targets = paragraphNode.SelectNodes(".//li[ul[not(li)]]");
                    if (targets == null)
                        continue;
                    foreach (var target in targets)
                    {
                        target.ParentNode.RemoveChild(target);
                    }
                }
            }
            se = doc.DocumentNode.OuterHtml;
            //8. TABLE相關處理
            doc.LoadHtml(se);
            //移除resize-sensor
            targetNodes = doc.DocumentNode.SelectNodes("//div[@class[contains(.,'resize-sensor')]]");
            if (targetNodes != null)
            {
                foreach (var targetNode in targetNodes)
                {
                    targetNode.ParentNode.RemoveChild(targetNode);
                }
            }
            //移除colresize
            targetNodes = doc.DocumentNode.SelectNodes("//div[@class[contains(.,'colresize')]]");
            if (targetNodes != null)
            {
                foreach (var targetNode in targetNodes)
                {
                    targetNode.ParentNode.RemoveChild(targetNode);
                }
            }
            //移除空表格 + 表格前後必須要有 <br>
            targetNodes = doc.DocumentNode.SelectNodes("//div[@class[contains(.,'tableContainer')]]");
            if (targetNodes != null)
            {
                foreach (var targetNode in targetNodes)
                {
                    var tableNode = targetNode.ChildNodes.FindFirst("table");
                    if (tableNode != null)
                    {
                        var tbodyNode = tableNode.ChildNodes.FindFirst("tbody");
                        if (tbodyNode == null)
                            targetNode.ParentNode.RemoveChild(targetNode);
                        else
                        {
                            if (targetNode.PreviousSibling == null || targetNode.PreviousSibling.OriginalName != "br")
                            {
                                targetNode.ParentNode.InsertBefore(HtmlNode.CreateNode("<br>"), targetNode);
                            }
                            if (targetNode.NextSibling == null || targetNode.NextSibling.OriginalName != "br")
                            {
                                targetNode.ParentNode.InsertAfter(HtmlNode.CreateNode("<br>"), targetNode);
                            }
                        }
                    }
                    else
                    {
                        targetNode.ParentNode.RemoveChild(targetNode);
                    }
                }
            }
            //9.空的td補上一個空白，防止重新算寬度的時候算錯
            var tds = doc.DocumentNode.SelectNodes("//td");
            if (tds != null)
            {
                foreach (var td in tds)
                {
                    //如果只有空白也會錯，所以加上trim
                    if (td.InnerHtml.Trim() == "" && td.InnerText.Trim() == "")
                        td.InnerHtml = "&nbsp;";
                    else if (td.HasChildNodes && td.InnerText.Trim() == "")
                    {
                        td.FirstChild.InnerHtml = "&nbsp;";
                    }
                }
            }
            //10. 最後回傳乾乾淨淨的SE
            se = doc.DocumentNode.OuterHtml;
            return se;
        }
        public string GetClearHtmlForExporter(string se)
        {
            var clearHtml = GetClearHtml(se);
            var doc = new HtmlDocument { OptionFixNestedTags = true };
            doc.LoadHtml(clearHtml);
            var targetNodes = doc.DocumentNode.SelectNodes("//div[table]");
            if (targetNodes != null)
            {
                //div>table前面若有float會多換行，把float的element移到最上面
                foreach (var targetNode in targetNodes)
                {
                    var maxLoopTime = 100;
                    while (targetNode.PreviousSibling != null && IsAbsolute(targetNode.PreviousSibling) && maxLoopTime > 0)
                    {
                        var floatNode = targetNode.PreviousSibling.Clone();
                        targetNode.ParentNode.RemoveChild(targetNode.PreviousSibling);
                        targetNode.ParentNode.PrependChild(floatNode);
                        maxLoopTime--;
                    }
                }

                //margin-top轉margin-bottom，且margin-bottom加在br上，沒有br就自己插入
                foreach (var targetNode in targetNodes)
                {
                    var style = GetStyles(targetNode);
                    if (!style.ContainsKey("margin-top"))
                        continue;
                    var previousNonFloatNode = GetPreviousNonFloatItem(targetNode.PreviousSibling);
                    if (previousNonFloatNode == null)
                        continue;
                    var previousStyleString = previousNonFloatNode.GetAttributeValue("style", "");
                    var marginBottom = Convert.ToDouble(style["margin-top"]);
                    //GSSSPEED-5005: 如果為含有table的div或段落ul則在後方插入<br>並加上margin-bottom
                    if ((previousNonFloatNode.Name == "div" &&
                        previousNonFloatNode.HasChildNodes &&
                        previousNonFloatNode.ChildNodes.Any(x => x.Name == "table")) ||
                        (previousNonFloatNode.Name == "ul" &&
                         previousNonFloatNode.GetAttributeValue("data-speed-id", "").Contains("paragraph")))
                    {
                        previousNonFloatNode.ParentNode.InsertAfter(HtmlNode.CreateNode("<br>"), previousNonFloatNode);
                        previousNonFloatNode = GetPreviousNonFloatItem(targetNode.PreviousSibling);
                        var previousFontSize = GetFontSize(previousNonFloatNode);
                        if (previousFontSize > marginBottom)
                        {
                            previousNonFloatNode.SetAttributeValue("data-speed-fontsize", marginBottom.ToString(CultureInfo.CurrentCulture));
                            marginBottom = 0;
                        }
                        else
                        {
                            marginBottom -= previousFontSize;
                        }
                    }
                    else
                    {
                        var previousStyle = GetStyles(previousNonFloatNode);
                        if (previousStyle.ContainsKey("margin-bottom"))
                        {
                            //按照瀏覽器邏輯: 誰margin比較大就聽誰的
                            var previousMarginBottom = Convert.ToDouble(previousStyle["margin-bottom"]);
                            marginBottom = previousMarginBottom > marginBottom ? previousMarginBottom : marginBottom;
                        }
                    }
                    previousNonFloatNode.SetAttributeValue("style", $"margin-bottom:{marginBottom}pt;{previousStyleString.Replace("margin-bottom", "margin-bottom1")}");
                    var styleString = targetNode.GetAttributeValue("style", "").Replace("margin-top", "margin-top1");
                    targetNode.SetAttributeValue("style", styleString);
                }
            }
            //td>table 如果沒colgroup要補上，讓內層表格的寬度能填滿整個TD
            targetNodes = doc.DocumentNode.SelectNodes("//td[table]");
            if (targetNodes != null)
            {
                foreach (var targetNode in targetNodes)
                {
                    var innerTables = targetNode.SelectNodes("./table");
                    if (innerTables == null)
                        continue;
                    foreach (var innerTable in innerTables)
                    {
                        //職章不需要處理
                        if (innerTable.Ancestors("div")
                                      .Any(x => x.GetAttributeValue("data-speed-id", "") == "seal-box"))
                            continue;
                        //如果已有col就不用處理
                        var cols = innerTable.SelectNodes("./colgroup/col") ??
                                   innerTable.SelectNodes("./col");
                        if (cols != null)
                            continue;
                        //表格寬度558.7mm是word的最大值，超過會壞掉
                        innerTable.AppendChild(HtmlNode.CreateNode("<col span='1' style='width:558.7mm;'>"));
                    }
                }
            }
            //table加上cellspacing=0
            targetNodes = doc.DocumentNode.SelectNodes("//table");
            if (targetNodes != null)
            {
                foreach (var targetNode in targetNodes)
                {
                    targetNode.SetAttributeValue("cellspacing", "0");
                }
            }
            //data-speed-control=dropdownlist清除多餘的element
            targetNodes = doc.DocumentNode.SelectNodes("//div[@data-speed-control='dropdownlist']/label");
            if (targetNodes != null)
            {
                foreach (var targetNode in targetNodes)
                {
                    //如果label有noprint或跟placeholder一樣就直接清空
                    if (targetNode.GetAttributeValue("class", "").Contains("noprint") ||
                        targetNode.ParentNode.GetAttributeValue("data-speed-placeholder", "") == targetNode.InnerHtml)
                        targetNode.InnerHtml = "";
                    targetNode.ParentNode.InnerHtml = targetNode.InnerHtml;
                }
            }
            //Modify by Stan 20181220: 補空白是為了匯出，不應存回SE，所以將邏輯搬過來
            //空的td補上一個空白，防止重新算寬度的時候算錯
            var tds = doc.DocumentNode.SelectNodes("//td");
            if (tds != null)
            {
                foreach (var td in tds)
                {
                    //如果只有空白也會錯，所以加上trim
                    if (td.InnerHtml.Trim() == "" && td.InnerText.Trim() == "")
                        td.InnerHtml = "&nbsp;";
                    //原用意是補td內div的空白，但範本有可能是td含table，如果補了內層table就會變空table，所以補上判斷條件
                    else if (td.HasChildNodes && td.InnerText.Trim() == "" && td.ChildNodes.All(c => c.Name != "table" && c.Name != "div"))
                    {
                        td.FirstChild.InnerHtml = "&nbsp;";
                    }
                }
            }
            return doc.DocumentNode.OuterHtml;
        }
        private HtmlNode GetPreviousNonFloatItem(HtmlNode previousSibling)
        {
            var current = previousSibling;
            while (current != null)
            {
                //GSSSPEED-5005: 如果在#text元素加上style會導致HAP元件當掉，且noprint元素也要一併忽略，否則加了style也會沒效果。
                if (IsAbsolute(current) || current.Name == "#text" || current.GetAttributeValue("class", "").Contains("noprint"))
                {
                    current = current.PreviousSibling;
                    continue;
                }
                break;
            }
            return current ?? previousSibling;
        }

        /// <summary>
        /// 從SE中讀取ContentEditor的寬度，單位為point(pt)
        /// </summary>
        /// <param name="content">SE內容(預設會用前次傳入的SE)
        /// </param>
        /// <returns></returns>
        public string GetContentWidth(string content = "")
        {
            if (_htmlDoc == null && content == String.Empty)
                return "453.555";

            if (content != String.Empty)
            {
                _htmlDoc = new HtmlDocument();
                _htmlDoc.LoadHtml(content);
            }

            var node = _htmlDoc?.DocumentNode.SelectSingleNode("//div[@class='ContentEditor']");
            var style = GetStyles(node);
            return style.ContainsKey("width") ? style["width"] : "453.555";
        }


        /// <summary>
        /// 目前只能概略的算出表格ROW實際的高度，可能會有些許誤差
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private double CalculateRowHeight(Row row)
        {
            var cellHeight = new List<double>();
            foreach (Cell cell in row.Cells.ToArray())
            {
                var height = 0D;
                var width = cell.CellFormat.Width;
                //foreach (Paragraph paragraph in cell.Paragraphs)
                //{
                //    var spacing = paragraph.ParagraphFormat.SpaceBefore + paragraph.ParagraphFormat.SpaceAfter;
                //    var leftIndent = paragraph.ParagraphFormat.LeftIndent;
                //    var fontSize = paragraph.ParagraphFormat.Style.Font.Size;
                //    var textWidth = paragraph.Runs.Cast<Run>().Sum(run => run.Font.Size * run.Text.Length);
                //    var remainWidth = width - leftIndent;
                //    height += Math.Ceiling(textWidth / remainWidth) * (fontSize + spacing);
                //}
                height += (from Paragraph paragraph in cell.Paragraphs let spacing = paragraph.ParagraphFormat.SpaceBefore + paragraph.ParagraphFormat.SpaceAfter let leftIndent = paragraph.ParagraphFormat.LeftIndent let fontSize = paragraph.ParagraphFormat.Style.Font.Size let textWidth = paragraph.Runs.Cast<Run>().Sum(run => run.Font.Size * run.Text.Length) let remainWidth = width - leftIndent select Math.Ceiling(textWidth / remainWidth) * (fontSize + spacing)).Sum();
                cellHeight.Add(height);
            }
            return cellHeight.Max();
        }

        private List<TableData> StoreTableWidthData(DocumentBuilder tempBuilder = null)
        {
            var result = new List<TableData>();
            var tables = _htmlDoc.DocumentNode.SelectNodes("//table");
            if (tables == null)
                return result;
            foreach (var table in tables)
            {
                var tableData = new TableData();
                var tds = table.SelectNodes(".//td");
                if (tds == null)
                {
                    result.Add(tableData);
                    continue;
                }

                //TableData -> SplitRow
                var trs = table.SelectNodes("./tbody/tr");
                if (trs != null)
                {
                    foreach (var tr in trs)
                    {
                        tableData.SplitRows.Add(Convert.ToBoolean(tr.GetAttributeValue("data-speed-splittr", "true") == "true"));
                    }
                }

                //TableData -> Type
                if (table.ParentNode.GetAttributeValue("data-speed-id", "") == "seal-box")
                {
                    tableData.Type = TableType.Stamp;
                    for (var i = 0; i < tableData.SplitRows.Count; i++)
                    {
                        tableData.SplitRows[i] = false;
                    }
                }
                else if (table.ParentNode.GetAttributeValue("data-speed-approvedbox", "") != "")
                {
                    tableData.Type = TableType.ApprovedBox;
                }
                else
                {
                    tableData.Type = TableType.Normal;
                }



                var tdList = new List<double>();
                var cols = table.SelectNodes("./colgroup/col") ??
                           table.SelectNodes("./col");

                //因為沒辦法相信td的順序是否為正確的，只好學瀏覽器用"猜"的
                //範本有rowspan的情況下，沒辦法確認該td是在rowspan前還是後
                //只好用算位置的方式，如果有空就硬塞td進去(不管colspan)，沒位置就自動向後塞
                //https://jsfiddle.net/t30fvnra/5/
                var rowspanHelper = new List<TableSpan>();

                foreach (var td in tds)
                {
                    //tr中可能有不是td的存在(不知道怎麼弄出來的)，為了防呆必須統一清除
                    var nonTdElements = td.ParentNode.ChildNodes.Where(c => c.OriginalName != "td").ToList();
                    foreach (var nonTdElement in nonTdElements)
                    {
                        td.ParentNode.RemoveChild(nonTdElement);
                    }
                    //tbody中可能有不是tr的存在，需統一清除
                    var nonTrElements = td.ParentNode.ParentNode.ChildNodes.Where(c => c.OriginalName != "tr").ToList();
                    foreach (var nonTrElement in nonTrElements)
                    {
                        td.ParentNode.ParentNode.RemoveChild(nonTrElement);
                    }

                    var tdParentTable = td.Ancestors().First(x => x.Name == "table");
                    if (tdParentTable != null && tdParentTable != table)
                    {
                        continue;
                    }

                    var style = GetStyles(td, tempBuilder);
                    if (style.ContainsKey("display") && style["display"] == "none")
                        continue;

                    var index = GetSpanIndex(0, td);
                    var span = Convert.ToInt32(td.GetAttributeValue("colspan", "1"));
                    var width = 0D;

                    //處理RowSpan (如果是章的rowspan要略過)
                    //var isStamp = td.GetAttributeValue("data-speed-sealusername", "") != "";
                    var isStamp = td.Ancestors()?.Any(x => x.GetAttributeValue("data-speed-id", "") == "seal-box") ?? false;
                    var rowIndex = td.ParentNode.ParentNode.ChildNodes.IndexOf(td.ParentNode);
                    var rowspan = Convert.ToInt32(td.GetAttributeValue("rowspan", "1"));
                    var colSpanLength = span;
                    //在有rowspan的情況下算出正確的spanIndex
                    //先算出自己同行td(含colspan)的index
                    var spanIndex = GetSpanIndex(index, td, rowspanHelper);
                    if (span == 0)
                    {
                        colSpanLength = (cols?.Count ?? td.ParentNode.ChildNodes.Count) - spanIndex;
                    }

                    if (rowspan == 0 && !isStamp)
                    {
                        //從自己合併到最後
                        var lastRowIndex = tds.Last().ParentNode.ParentNode.ChildNodes.IndexOf(tds.Last().ParentNode);
                        rowspanHelper.Add(new TableSpan(rowIndex, lastRowIndex - rowIndex, spanIndex, colSpanLength));
                    }
                    else if (rowspan > 1 && !isStamp)
                    {
                        //從rowIndex合併到指定長度
                        rowspanHelper.Add(new TableSpan(rowIndex, rowspan, spanIndex, colSpanLength));
                    }

                    if (style.ContainsKey("width"))
                        width = Convert.ToDouble(style["width"]);
                    else
                    {
                        var currentCols = cols;
                        //在有rowspan的情況下算出正確的spanIndex
                        //先算出自己同行td(含colspan)的index
                        //var spanIndex = spanIndex;
                        //rowspanHelper.RemoveAll(x => x.RowIndex + x.RowSpanLength <= rowIndex);
                        ////如果有rowspan的資料就再加上rowspan => 這樣就可以得到正確的index
                        //if (rowspanHelper.Any())
                        //{
                        //    foreach (var current in rowspanHelper)
                        //    {
                        //        //同行的rowspan就不用算了，因為上面GetSpanIndex會算到
                        //        if (current.RowIndex == rowIndex)
                        //            continue;

                        //        //如果rowspan的起始index(ColIndex)比現在td小的話，代表這個td之前有這個rowspan，所以要加上這個rowspan的長度
                        //        if (current.ColIndex <= spanIndex)
                        //            spanIndex += current.ColSpanLength;
                        //    }
                        //}

                        if (currentCols != null && currentCols.Count > spanIndex && spanIndex >= 0)
                        {
                            //沒有合併的狀況，只算自己col
                            if (span == 1)
                            {
                                var colStyle = GetStyles(currentCols[spanIndex], tempBuilder);
                                if (colStyle.ContainsKey("width"))
                                    double.TryParse(colStyle["width"], out width);
                            }
                            //有合併，需加總col的寬度，另外加上防呆
                            else if (span > 1 && spanIndex + span <= currentCols.Count)
                            {
                                var tempWidth = 0D;
                                for (var i = spanIndex; i < spanIndex + span; i++)
                                {
                                    var colStyle = GetStyles(currentCols[i], tempBuilder);
                                    if (colStyle.ContainsKey("width"))
                                        double.TryParse(colStyle["width"], out tempWidth);
                                    width += tempWidth;
                                }
                            }
                            //colspan=0 (從自己合併到最後)，或是colspan的值有錯(超過col的數量)
                            else
                            {
                                var tempWidth = 0D;
                                for (var i = spanIndex; i < currentCols.Count; i++)
                                {
                                    var colStyle = GetStyles(currentCols[spanIndex], tempBuilder);
                                    if (colStyle.ContainsKey("width"))
                                        double.TryParse(colStyle["width"], out tempWidth);
                                    width += tempWidth;
                                }
                            }
                        }
                    }
                    tdList.Add(width);
                }
                tableData.CellWidths = tdList;
                result.Add(tableData);
            }
            return result;
        }
        private int GetSpanIndex(int index, HtmlNode td, List<TableSpan> helper = null)
        {
            var rowIndex = td.ParentNode.ParentNode.ChildNodes.IndexOf(td.ParentNode);
            var spanIndex = 0;
            //if (helper != null && helper.Any())
            //{
            //    helper.RemoveAll(x => x.RowIndex + x.RowSpanLength <= rowIndex);
            //    var iterator = helper.GetEnumerator();
            //    var current = iterator.Current;
            //    while (iterator.MoveNext() && iterator.Current != null)
            //    {
            //        //同tr的不用計算，不同tr才需要考慮rowspan的問題
            //        if (iterator.Current.RowIndex == rowIndex)
            //        {
            //            continue;
            //        }
            //        //兩個rowspan之間或第一個rowspan之前如果有空格可以插的話就直接插，不用colspan是否超過空格寬度
            //        if (current != null && (current.HasSlot(iterator.Current) || current.ColIndex > index))
            //        {
            //            break;
            //        }
            //        //rowspan在目前td之後，所以直接插
            //        if (current == null && iterator.Current.ColIndex > GetSpanIndex(index, td))
            //        {
            //            break;
            //        }
            //        spanIndex = iterator.Current.ColIndex + iterator.Current.ColSpanLength;
            //        current = iterator.Current;
            //    }
            //    iterator.Dispose();
            //}
            foreach (var tdInRow in td.ParentNode.ChildNodes)
            {
                var tempStyle = GetStyles(tdInRow);
                if (tempStyle.ContainsKey("display") && tempStyle["display"] == "none")
                    continue;
                if (tdInRow == td)
                    break;
                spanIndex += Convert.ToInt32(tdInRow.GetAttributeValue("colspan", "1"));
            }

            if (helper != null && helper.Any())
            {
                helper.RemoveAll(x => x.RowIndex + x.RowSpanLength <= rowIndex);
                //如果有rowspan的資料就再加上rowspan => 這樣就可以得到正確的index
                foreach (var current in helper)
                {
                    //同行的rowspan就不用算了，因為上面GetSpanIndex會算到
                    if (current.RowIndex == rowIndex)
                        continue;

                    //如果rowspan的起始index(ColIndex)比現在td小的話，代表這個td之前有這個rowspan，所以要加上這個rowspan的長度
                    if (current.ColIndex <= spanIndex)
                        spanIndex += current.ColSpanLength;
                }
            }

            if (spanIndex == 0)
                spanIndex = index;
            return spanIndex;
        }

        private void RemoveLastReturn(DocumentBuilder builder = null, int maxLoopCount = 100)
        {
            if (builder == null)
                builder = _builder;
            var loopCount = 0;
            var removeTarget = builder.CurrentParagraph;
            while (!removeTarget.GetText().Contains("\r"))
            {
                removeTarget = (Paragraph)removeTarget.PreviousSibling;
                if (++loopCount > maxLoopCount)
                {
                    removeTarget = null;
                    break;
                }
            }
            if (removeTarget != null && removeTarget.GetText() == "\r")
                removeTarget.Remove();
            else if (removeTarget != null && removeTarget.HasChildNodes)
            {
                for (var i = removeTarget.ChildNodes.Count - 1; i > 0; i--)
                {
                    var childNode = removeTarget.ChildNodes[i];
                    if (childNode.GetText() == "\r")
                    {
                        childNode.Remove();
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 根據ItemStyle取得對應的階層
        /// </summary>
        /// <param name="itemStyle"></param>
        /// <returns></returns>
        private int GetItemLevel(string itemStyle)
        {
            var style1 = new[] { "一", "二", "三", "四", "五", "六", "七", "八", "九", "零", "十", "百", "千" };
            var style2 = new[] { "１", "２", "３", "４", "５", "６", "７", "８", "９", "０" };
            var style3 = new[] { "甲", "乙", "丙", "丁", "戊", "己", "庚", "辛", "壬", "癸" };
            var style4 = new[] { "子", "丑", "寅", "卯", "辰", "巳", "午", "未", "申", "酉", "戌", "亥" };

            string clear;
            int index;
            if (itemStyle.Contains("、"))
            {
                clear = itemStyle.Replace("、", "");
                index = 1;
            }
            else
            {
                clear = itemStyle.Replace("(", "").Replace(")", "");
                index = 2;
            }

            if (string.IsNullOrEmpty(clear))
                return 9;

            if (clear.All(x => style1.Contains(x.ToString())))
                return index;

            if (clear.All(x => style2.Contains(x.ToString())))
                return index + 2;

            if (clear.All(x => style3.Contains(x.ToString())))
                return index + 4;

            if (clear.All(x => style4.Contains(x.ToString())))
                return index + 6;

            return 9;
        }
        /// <summary>
        /// 根據階層取得對應的ItemStyle
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private IEnumerable<string> GetItemStyle(int target)
        {
            var dictionary = new List<string[]>
            {
                new[] {"一", "二", "三", "四", "五", "六", "七", "八", "九", "十", "百", "千"},
                new[] {"１", "２", "３", "４", "５", "６", "７", "８", "９", "０"},
                new[] {"甲", "乙", "丙", "丁", "戊", "己", "庚", "辛", "壬", "癸"},
                new[] {"子", "丑", "寅", "卯", "辰", "巳", "午", "未", "申", "酉", "戌", "亥"},
            };

            if (target > 8 || target < 1)
                return new[] { "" };

            var style = dictionary[target / 2 + target % 2 - 1];
            return style.Select(x => target % 2 == 1 ? $"{x}、" : $"({x})");
        }

        private string ReturnCleanASCII(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (var c in s)
            {
                if ((int)c > 127) // you probably don't want 127 either
                    continue;
                if ((int)c < 32)  // I bet you don't want control characters 
                    continue;
                sb.Append(c);
            }
            return sb.ToString();
        }
        /// <summary>
        /// 將Exporter發生的錯誤，獨立紀錄於ExporterLog底下
        /// </summary>
        /// <param name="message">Exception訊息</param>
        /// <param name="elmahPath">ElmahLogPath</param>
        /// <param name="title">自訂錯誤訊息</param>
        public void LogError(string message, string elmahPath, string title = "")
        {
            var logPath = Path.Combine(elmahPath, "ExporterLog");
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            var fileName = $"error-{DateTimeUtil.Now.Ticks}.txt";
            var content = $"{title}\r\n{message}\r\nSE內容：\r\n{_htmlDoc?.DocumentNode?.OuterHtml ?? "EMPTY"}";
            File.WriteAllText(Path.Combine(logPath, fileName), content);
        }
    }
}
