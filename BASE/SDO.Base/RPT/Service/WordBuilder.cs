using Aspose.Words;
using Aspose.Words.Layout;
using Autofac;
using SDO.ReportBuilder.Interface;
using SDO.ReportBuilder.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SDO.ReportBuilder.Services
{
    public abstract class WordBuilder : WordService, IRPTBuilder
    {
        private readonly IComponentContext coms;

        /// <summary>
        /// Aspose WordBuilder物件
        /// </summary>
        public WordBuilder(IComponentContext coms) : base(coms)
        {
            this.coms = coms;
        }

        /// <summary>
        /// 建立內容
        /// </summary>
        /// <returns></returns>
        protected abstract Task<bool> MakeContent();

        /// <summary>
        /// 建立報表
        /// </summary>
        /// <param name="parameter">報表參數</param>
        /// <returns></returns>
        public virtual async Task<(MemoryStream ms, string outputName, string mime)> Create(RptParameter parameter)
        {
            if (parameter.IsDiffCompare)
            {
                return await CreateDiffCompare(parameter);
            }
            else
            {
                await ProduceDoc(parameter);

                return Save();
            }
        }

        private async Task ProduceDoc(RptParameter parameter)
        {
            Parameter = parameter;
            Doc = new Document();
            Builder = new DocumentBuilder(Doc);

            await GetData();
            await MakeContent();
            //清除整份文件未取代的tag
            wordSetService.CleanTag(Doc);
        }

        /// <summary>
        /// 建立差異比對報表
        /// </summary>
        /// <param name="parameter">報表參數</param>
        /// <returns></returns>
        public virtual async Task<(MemoryStream ms, string outputName, string mime)> CreateDiffCompare(RptParameter parameter)
        {
            List<Document> docs = new List<Document>();

            List<int> data = (List<int>)parameter.ObjectModel;
            for (int x = 0; x < data.Count(); x++)
            {
                parameter.DiffId = data[x];
                await ProduceDoc(parameter);
                docs.Add(Doc);
            }

            if (docs.Count > 1)
            {
                CompareOptions options = new CompareOptions();
                options.Target = ComparisonTargetType.New;

                // 比對文件
                docs[0].Compare(docs[1], "SYSTEM", DateTime.Now, options);
               
                docs[0].LayoutOptions.RevisionOptions.DeletedTextEffect = RevisionTextEffect.None;
                docs[0].LayoutOptions.RevisionOptions.InsertedTextEffect = RevisionTextEffect.None;
                docs[0].LayoutOptions.RevisionOptions.ShowRevisionBars = false;
            }
          
            NodeCollection runs = docs[0].GetChildNodes(NodeType.Run, true);
            foreach (Run run in runs)
            {
                // 第一筆修改前(刪除線，紅色呈現)
                if (run.IsDeleteRevision)
                {
                    run.Font.Color = Color.Red;
                    run.Font.StrikeThrough =true;
                }
                // 第二筆修改後(藍色呈現)
                if (run.IsInsertRevision)
                {
                    run.Font.Color = Color.Blue;
                    run.Font.Underline = Underline.None;
                }
            }

            return Save(docs[0]);
        }

        protected virtual Task GetData()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 存檔
        /// </summary>
        /// <returns></returns>
        protected virtual (MemoryStream ms, string outputName, string mime) Save(Document doc = null)
        {
            if (doc != null)
            {
                Doc = doc;
            }
            if (Doc == null)
            {
                throw new Exception("報表產生失敗");
            }

            Builder.PageSetup.Orientation = Parameter.PrintOri;
            Builder.PageSetup.PaperSize = Parameter.paperSize;

            var stream = new MemoryStream();
            string mimeType = string.Empty;
            switch (string.IsNullOrEmpty(Parameter.Extension) ? string.Empty : Parameter.Extension.ToLower())
            {
                case "odt":
                    mimeType = UCTableExport.MIME_ODT;
                    Doc.Save(stream, SaveFormat.Odt);
                    break;
                case "pdf":
                    mimeType = UCTableExport.MIME_PDF;
                    Doc.Save(stream, SaveFormat.Pdf);
                    break;
                case "html":
                    mimeType = UCTableExport.MIME_HTML;
                    Doc.Save(stream, SaveFormat.Html);
                    break;
                default:
                    mimeType = UCTableExport.MIME_DOCX;
                    Parameter.Extension = "docx";
                    Doc.Save(stream, SaveFormat.Docx);
                    break;
            }
            stream.Position = 0;
            return (stream, $"{Parameter.FileName}.{Parameter.Extension}", mimeType);
        }

        /// <summary>
        /// 取得報表執行各體
        /// </summary>
        /// <param name="pConstruct">建構所需物件參數</param>
        /// <param name="className">製表clasName</param>
        /// <returns></returns>
        protected IContentBuilder CreateService<T>(object[] pConstruct, string className)
        {
            var parameter = (RptParameter)pConstruct.Where(x => x is RptParameter).FirstOrDefault();
            var docBuilder = (DocumentBuilder)pConstruct.Where(x => x is DocumentBuilder).FirstOrDefault();

            var theAssembly = Assembly.GetAssembly(typeof(T));
            Type theType = theAssembly.GetType($"{typeof(T).Namespace}.{className}");

            var RptService = (IContentBuilder)Activator.CreateInstance(theType,
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new object[] { coms },
                System.Globalization.CultureInfo.InvariantCulture,
                null);
            RptService.InjectParameter(docBuilder, parameter);
            return RptService;
        }

        /// <summary>
        /// 初始化文件產生器（Doc Builder）
        /// </summary>
        /// <param name="doc">文件</param>
        /// <returns>文件產生器（Doc Builder）</returns>
        protected DocumentBuilder InitBuilder(DocConfigModel config = null)
        {
            //預設字型
            Builder.Font.Name = string.IsNullOrEmpty(config?.DefaultFontName) ? defaultFontName : config.DefaultFontName;
            //中文字型
            Builder.Font.NameFarEast = string.IsNullOrEmpty(config?.DefaultFontNameForEast) ? defaultFontNameFarEast : config.DefaultFontNameForEast;
            //符號使用之字體，用於核取方塊
            Builder.Font.NameOther = string.IsNullOrEmpty(config?.DefaultFontNameForEast) ? defaultFontNameFarEast : config.DefaultFontNameForEast;
            Builder.Font.Size = defaultFontSize;
            Builder.PageSetup.TopMargin = 57;
            Builder.PageSetup.BottomMargin = 57;
            Builder.PageSetup.LeftMargin = 57;
            Builder.PageSetup.RightMargin = 57;

            // 判斷是否初始頁碼
            if (config != null && config.IsShowPager)
                InitPager(config);

            return Builder;
        }

        /// <summary>
        /// 初始頁碼
        /// </summary>
        /// <param name="builder">文件產生器（Doc Builder）</param>
        protected void InitPager(DocConfigModel configModel = null)
        {
            configModel = configModel ?? new DocConfigModel()
            {
                IsShowPager = true,
                PagerNumberType = "1",
            };

            if (!configModel.IsShowPager)
            {
                return;
            }

            Builder.MoveToHeaderFooter(HeaderFooterType.FooterPrimary);
            Builder.CurrentParagraph.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            Builder.Font.Size = defaultFontSize;
            switch (configModel.PagerNumberType)
            {
                case "2":
                    //純數字
                    Builder.Font.Name = defaultFontName;
                    Builder.InsertField("PAGE", string.Empty);
                    break;
                case "3":
                    //第../..頁
                    Builder.Font.Name = defaultFontName;
                    Builder.Font.NameFarEast = defaultFontNameFarEast;
                    Builder.Write("第 ");
                    Builder.InsertField("PAGE", string.Empty);
                    Builder.Write("/");
                    Builder.InsertField("NUMPAGES", string.Empty);
                    Builder.Write(" 頁");
                    break;
                case "4":
                    //前面特殊字串+數字                    
                    Builder.Font.Name = defaultFontName;
                    Builder.Write(configModel.PageNumberTitle ?? string.Empty);
                    Builder.InsertField("PAGE", string.Empty);
                    break;
                case "1":
                default:
                    //第..頁，共..頁
                    Builder.Font.Name = defaultFontName;
                    Builder.Font.NameFarEast = defaultFontNameFarEast;
                    Builder.Write("第 ");
                    Builder.InsertField("PAGE", string.Empty);
                    Builder.Write(" 頁，共 ");
                    Builder.InsertField("NUMPAGES", string.Empty);
                    Builder.Write(" 頁");
                    break;
            }
            Builder.MoveToDocumentEnd();
        }
    }
}
