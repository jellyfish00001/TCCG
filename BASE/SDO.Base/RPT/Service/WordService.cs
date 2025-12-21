using Aspose.Words;
using Aspose.Words.Fields;
using Aspose.Words.Tables;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using SDO.Models;
using SDO.ReportBuilder.Models;
using SDO.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.ReportBuilder.Services
{
    public abstract class WordService
    {
        protected readonly IWordSetService wordSetService;
        protected readonly IWebHostEnvironment webHostEnvironment;
        protected readonly ISetParamService sysParam;

        public WordService(IComponentContext coms)
        {
            this.wordSetService = coms.Resolve<IWordSetService>();
            this.webHostEnvironment = coms.Resolve<IWebHostEnvironment>();
            this.sysParam = coms.Resolve<ISetParamService>();
        }

        /// <summary>
        /// 預設字型
        /// </summary>
        protected string defaultFontName = "標楷體";

        /// <summary>
        /// 預設中文字型
        /// </summary>
        protected string defaultFontNameFarEast = "標楷體";

        /// <summary>
        /// 預設文字大小
        /// </summary>
        protected double defaultFontSize = 12;

        /// <summary>
        /// 標題文字大小
        /// </summary>
        protected const double titleFontSize = 16;

        /// <summary>
        /// 副標題文字大小
        /// </summary>
        protected const double subTitleFontSize = 14;

        /// <summary>
        /// 備註文字大小
        /// </summary>
        protected const double remarkFontSize = 10;

        /// <summary>
        /// 套表參數
        /// </summary>
        protected object BasicData { get; set; }

        /// <summary>
        /// 列表物件，支援多table套表
        /// </summary>
        protected IList<ITableData> ListData { get; set; } = new List<ITableData>();

        /// <summary>
        /// 範本檔路徑
        /// </summary>
        protected string TemplatePath { get; set; }

        /// <summary>
        /// 樣板名稱
        /// </summary>
        protected string TemplateFileName { get; set; }

        /// <summary>
        /// Word物件
        /// </summary>
        protected Document Doc { get; set; }

        /// <summary>
        /// Aspose WordBuilder物件
        /// </summary>
        protected DocumentBuilder Builder { get; set; }

        /// <summary>
        /// table
        /// </summary>
        protected Table Table { get; set; }

        /// <summary>
        /// 報表參數
        /// </summary>
        protected RptParameter Parameter { get; set; }

        /// <summary>
        /// 取得核取方塊 
        /// </summary>
        /// <param name="isCheck">是否勾選</param>
        /// <returns>true■，false□</returns>
        protected string GetCheckedYesNo(bool isCheck)
        {
            // true: 0x25A0■ ; false: 0x25A1□
            return isCheck ? ((char)0x25A0).ToString() : ((char)0x25A1).ToString();
        }

        /// <summary>
        /// 新增標題(含連結)
        /// </summary>
        /// <param name="displayText">連結文字</param>
        /// <param name="url">連結</param>
        /// <returns>連結文件欄位</returns>
        protected Field InsertCaptionLink(string displayText, string url)
        {
            Builder.MoveToDocumentEnd();
            Color oriFontColor = Builder.Font.Color;
            Underline oriUnderline = Builder.Underline;
            Builder.Font.Underline = Underline.Single;
            Builder.Font.Size = 14;
            Builder.Font.Bold = true;
            Builder.ParagraphFormat.SpaceBefore = 10;
            Builder.ParagraphFormat.SpaceBefore = 0;
            Field field = Builder.InsertHyperlink(displayText + "\n", url, false);
            Builder.Font.Color = oriFontColor;
            Builder.Font.Underline = oriUnderline;
            return field;
        }

        /// <summary>
        /// 插入內文在 DocumentBuilder 中
        /// </summary>
        /// <param name="contentStr">內文字串</param>
        /// <param name="size">文字大小，若沒給則使用 DocumentCell預設</param>
        /// <param name="fontColor"></param>
        protected void InsertBuilderContent(string contentStr, double? size = null, Color? fontColor = null)
        {
            Color colorOrigin = Builder.Font.Color;
            if (fontColor != null)
                Builder.Font.Color = (Color)fontColor; //設定標記顏色
            Builder.Bold = false;
            Builder.Font.Size = size ?? defaultFontSize;
            Builder.Write(contentStr);
            Builder.Font.Color = colorOrigin; //恢復原始顏色
        }

        /// <summary>
        /// 設定標題
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="fontSize">字型大小</param>
        /// <param name="bold">粗體</param>
        /// <param name="underline">底線</param>
        /// <param name="isWrap">是否換行</param>
        /// <param name="alignment">文字對齊值</param>
        protected void SetTitle(string value, double fontSize = titleFontSize, bool bold = false, Underline underline = Underline.None, bool isWrap = true, ParagraphAlignment alignment = ParagraphAlignment.Left)
        {
            Builder.ParagraphFormat.Alignment = alignment;
            Builder.ParagraphFormat.LineUnitAfter = 1; // 與後段距離1行
            Builder.Font.Size = fontSize;
            Builder.Font.Name = defaultFontNameFarEast;
            Builder.Bold = bold;
            Builder.Font.Underline = underline;
            if (isWrap)
            {
                Builder.Writeln(value);
            }
            else
            {
                Builder.Write(value);
            }
            ClearFormat();
        }

        /// <summary>
        /// 設定副標題
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="fontSize">字型大小</param>
        /// <param name="bold">粗體</param>
        /// <param name="underline">底線</param>
        /// <param name="isWrap">是否換行</param>
        /// <param name="alignment">文字對齊值</param>
        /// <param name="firstLineIndent">首行縮排</param>
        protected void SetSubTitle(string value, double fontSize = subTitleFontSize, bool bold = false, Underline underline = Underline.None, bool isWrap = true, ParagraphAlignment alignment = ParagraphAlignment.Left, double firstLineIndent = 0)
        {
            Builder.ParagraphFormat.CharacterUnitFirstLineIndent = firstLineIndent;
            Builder.ParagraphFormat.Alignment = alignment;
            Builder.Font.Size = fontSize;
            Builder.Bold = bold;
            Builder.Font.Underline = underline;
            if (isWrap)
            {
                Builder.Writeln(value);
            }
            else
            {
                Builder.Write(value);
            }
            ClearFormat();
        }

        /// <summary>
        /// 設定備註
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="fontSize">字型大小</param>
        /// <param name="bold">粗體</param>
        /// <param name="underline">底線</param>
        /// <param name="isWrap">是否換行</param>
        /// <param name="alignment">文字對齊值</param>
        /// <param name="firstLineIndent">首行縮排</param>
        protected void SetRemark(string value, double fontSize = remarkFontSize, bool bold = false, Underline underline = Underline.None, bool isWrap = true, ParagraphAlignment alignment = ParagraphAlignment.Left, double firstLineIndent = 0)
        {
            Builder.ParagraphFormat.CharacterUnitFirstLineIndent = firstLineIndent;
            Builder.ParagraphFormat.Alignment = alignment;
            Builder.Font.Size = fontSize;
            Builder.Bold = bold;
            Builder.Font.Underline = underline;
            if (isWrap)
            {
                Builder.Writeln(value);
            }
            else
            {
                Builder.Write(value);
            }
            ClearFormat();
        }

        /// <summary>
        /// 格式恢復預設值
        /// </summary>
        protected void ClearFormat()
        {
            Builder.ParagraphFormat.ClearFormatting();
            Builder.Font.Size = defaultFontSize;
            Builder.Bold = false;
            Builder.Font.Underline = Underline.None;
        }
    }
}
