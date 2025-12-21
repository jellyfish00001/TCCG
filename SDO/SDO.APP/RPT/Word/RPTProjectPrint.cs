using Aspose.Words;
using Autofac;
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
    public class RPTProjectPrint : WordBuilder
    {
        private readonly IProjectPrintService projectPrintService;
        public RPTProjectPrint(IComponentContext coms) : base(coms)
        {
            this.projectPrintService = coms.Resolve<IProjectPrintService>();
        }

        protected override async Task<bool> MakeContent()
        {
            InitBuilder(new DocConfigModel
            {
                IsShowPager = Parameter.Extension == "html" ? false : true,
                PagerNumberType = "2"
            });
            Builder.PageSetup.TopMargin = 36;
            Builder.PageSetup.BottomMargin = 36;
            Builder.PageSetup.LeftMargin = 36;
            Builder.PageSetup.RightMargin = 36;
            SetTitle(Parameter.FileName, fontSize: 14, bold: true, alignment: ParagraphAlignment.Center);
            SetSubTitle($"資料日期：{DateTime.Now.ToTwDateString()}", fontSize: 12, alignment: ParagraphAlignment.Right);
            object[] objData = new object[] { Parameter, Builder };
            await CreateService<RPTProjectPrint>(objData, "ProjectFillBasic").MakeContent();
            Builder.Writeln(string.Empty);
            if (Parameter.Type == "A")
            {
                await CreateService<RPTProjectPrint>(objData, "ProjectCheckPoint").MakeContent();
            }
            else
            {
                Dictionary<string, object> showList = await projectPrintService.GetProjectPrintShowData(Parameter.PROJECT_NO);
                await CreateService<RPTProjectPrint>(objData, "ProjectCkptCom").MakeContent();
                Builder.Writeln(string.Empty);
                SetTitle("三、執行情形", fontSize: 14);

                if (showList.ContainsKey("ProjectExecute"))
                {
                    SetTitle(showList["ProjectExecute"].ToString(), fontSize: 14);
                    await CreateService<RPTProjectPrint>(objData, "ProjectFillExecute").MakeContent();
                    Builder.Writeln(string.Empty);
                }

                if (showList.ContainsKey("ProjectDelay"))
                {
                    SetTitle(showList["ProjectDelay"].ToString(), fontSize: 14);
                    await CreateService<RPTProjectPrint>(objData, "ProjectDelay").MakeContent();
                    Builder.Writeln(string.Empty);
                }

                if (showList.ContainsKey("ProjectBudgetExec"))
                {
                    SetTitle(showList["ProjectBudgetExec"].ToString(), fontSize: 14);
                    await CreateService<RPTProjectPrint>(objData, "ProjectBudgetExec").MakeContent();
                    Builder.Writeln(string.Empty);
                }

                if (showList.ContainsKey("ProjectField"))
                {
                    SetTitle(showList["ProjectField"].ToString(), fontSize: 14);
                    await CreateService<RPTProjectPrint>(objData, "ProjectField").MakeContent();
                    Builder.Writeln(string.Empty);
                }

                if (showList.ContainsKey("ProjectOther"))
                {
                    Parameter.ObjectModel = showList["SubProjectOther"];
                    SetTitle(showList["ProjectOther"].ToString(), fontSize: 14);
                    await CreateService<RPTProjectPrint>(objData, "ProjectOther").MakeContent();
                    Builder.Writeln(string.Empty);
                }

                if (showList.ContainsKey("ProjectClose"))
                {
                    SetTitle(showList["ProjectClose"].ToString(), fontSize: 14);
                    await CreateService<RPTProjectPrint>(objData, "ProjectClose").MakeContent();
                }

                if (Parameter.IsRdecFun)
                {
                    string[] number = new string[] { "一", "二", "三", "四" };
                    int count = 0;
                    Builder.Writeln(string.Empty);
                    SetTitle("四、管考備註", fontSize: 14);
                    SetTitle($"（{number[count++]}）平時管考意見", fontSize: 14);
                    await CreateService<RPTProjectPrint>(objData, "ProjectAudit").MakeContent();

                    if (showList.ContainsKey("ProjectField"))
                    {
                        Builder.Writeln(string.Empty);
                        SetTitle($"（{number[count++]}）實地查證意見", fontSize: 14);
                        await CreateService<RPTProjectPrint>(objData, "ProjectField").MakeContent();
                    }

                    Builder.Writeln(string.Empty);
                    Parameter.ObjectModel = ++count;
                    await CreateService<RPTProjectPrint>(objData, "ProjectAuditClose").MakeContent();
                }
            }
            return true;
        }
    }
}
