using Aspose.Words;
using Aspose.Words.Lists;
using Autofac;
using SDO.Models;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Services;
using SDO.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Word
{
    public class RPTInnProjectPrint : WordBuilder
    {
        private readonly IInnProjectService innProjectService;

        public RPTInnProjectPrint(IComponentContext coms) : base(coms)
        {
            this.innProjectService = coms.Resolve<IInnProjectService>();
        }

        protected override async Task<bool> MakeContent()
        {
            if (!Parameter.PROJECT_NO_DATA.Any())
            {
                throw new Exception("無檔案可下載");
            }
            InitBuilder(new DocConfigModel
            {
                IsShowPager = true,
                PagerNumberType = "2"
            });
            Builder.PageSetup.TopMargin = 36;
            Builder.PageSetup.BottomMargin = 36;
            Builder.PageSetup.LeftMargin = 36;
            Builder.PageSetup.RightMargin = 36;

            Document doc = Builder.Document;

            SetTitle(Parameter.FileName+"目錄", fontSize: 18, bold: true, alignment: ParagraphAlignment.Center);

            // 插入目錄
            Builder.InsertTableOfContents("\\o \"1-3\" \\h \\z \\u");

            List list = doc.Lists.Add(ListTemplate.NumberDefault);
            //编號格式
            list.ListLevels[0].NumberFormat = "\x0000.";
            list.ListLevels[0].NumberStyle = NumberStyle.Arabic;

            Font font = list.ListLevels[0].Font;
            font.Size = 14;
            font.Name = "Times New Roman";

            doc.Styles[StyleIdentifier.Toc1].Font.Size = 14;
            Style toc1Style = doc.Styles[StyleIdentifier.Toc1];
            toc1Style.ListFormat.List = list;


            foreach (var projectNO in Parameter.PROJECT_NO_DATA)
            {
                InnProjectBasicFillModel data = await innProjectService.GetInnBasic(projectNO);
                Builder.InsertBreak(BreakType.PageBreak);
                SetTitle("編號：" + projectNO, fontSize: 14, bold: true, alignment: ParagraphAlignment.Left);
                Builder.ParagraphFormat.StyleIdentifier = StyleIdentifier.Heading1;
                SetSubTitle(data.InnProjectBasic.INN_PLAN_NAME, fontSize: 14, bold: true, alignment: ParagraphAlignment.Center);
                Parameter.PROJECT_NO = projectNO;
                object[] objData = new object[] { Parameter, Builder };
                await CreateService<RPTInnProjectPrint>(objData, "InnProjectFillBasic").MakeContent();
                Builder.Writeln(string.Empty);
                // 更新目錄
                doc.UpdateFields();

            }


            return true;
        }
    }
}
