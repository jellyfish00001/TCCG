using Aspose.Words;
using Autofac;
using SDO.APP.IPC.Models.Statistics;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Services;
using SDO.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Word
{
    public class RPTIPCProjectADJ : WordBuilder
    {
        private readonly IStatisticsService service;
        public RPTIPCProjectADJ(IComponentContext coms) : base(coms)
        {
            this.service = coms.Resolve<IStatisticsService>();
        }

        protected override async Task<bool> MakeContent()
        {
            InitBuilder(new DocConfigModel
            {
                IsShowPager = true,
            });
            object[] objData = new object[] { Parameter, Builder };

            // 簡版
            if (Parameter.Type == "Short")
            {
                await CreateService<RPTExportWord>(objData, $"IPCProjectADJShort").MakeContenByTemplate();
            }
            // 詳版
            else if (Parameter.Type == "Detailed")
            {
                // 因範本 IPCProjectADJDetailedRunwayC 在套表之後，紙張大小會變成 Letter 的大小，故在此設定紙張大小為A4
                Builder.PageSetup.PaperSize = PaperSize.A4;
                
                // 設定標題
                SetTitle(Parameter.FileName, alignment: ParagraphAlignment.Center);
                List<ProjectAdjustDetailedModel> data = await GetData(Parameter.PROJECT_NO);
                foreach (ProjectAdjustDetailedModel item in data)
                {
                    Parameter.PROJ_ADJ_ID = item.PROJ_ADJ_ID;
                    Parameter.ObjectModel = item;
                    // 當該次調整有更換執行方式，則用範本 IPCProjectADJDetailedRunwayC，否則用 IPCProjectADJDetailedCheckpoint
                    string content = item.RUNWAY_C_ORI != item.RUNWAY_C ? "RunwayC" : "Checkpoint";
                    await CreateService<RPTExportWord>(objData, $"IPCProjectADJDetailed{content}").MakeContenByTemplate();
                    if (item != data.Last())
                    {
                        Builder.InsertBreak(BreakType.SectionBreakNewPage);
                    }
                    // 無期程調整過
                    if (item.PROJ_ADJ_ID == 0)
                    {
                        // 刪「調整歷程」以下的row
                        Builder.DeleteRow(0, 5);
                        Builder.DeleteRow(0, 5);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 取得每次調整是否修正執行方式
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <returns></returns>
        private async Task<List<ProjectAdjustDetailedModel>> GetData(string PROJECT_NO)
        {
            return await service.GetProjAdjDetailed(PROJECT_NO);
        }

    }
}
