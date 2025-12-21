using Aspose.Words;
using Autofac;
using SDO.APP.RD.Models.Report;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Services;
using SDO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Word
{
    public class RPTRDPlanExecutionSurvey : WordBuilder
    {
        /// <summary>
        /// 續列管委託研究計畫成果及運用情形調查表
        /// </summary>
        private readonly IRDReportDacDac rdReportDac;
        public RPTRDPlanExecutionSurvey(IComponentContext coms) : base(coms)
        {
            this.rdReportDac = coms.Resolve<IRDReportDacDac>();
        }

        protected override async Task<bool> MakeContent()
        {
            ReportQueryModel model = (ReportQueryModel)Parameter.ObjectModel;
            //抓取符合條件的PLAN_NO
            List<PlanResultModel> data = await rdReportDac.GetPlanResult(model);
            if (!data.Any())
            {
                throw new Exception("無檔案可下載");
            }
            InitBuilder(new DocConfigModel
            {
                IsShowPager = true
            });

            Builder.PageSetup.TopMargin = 42;
            Builder.PageSetup.BottomMargin = 42;
            Builder.PageSetup.LeftMargin = 42;
            Builder.PageSetup.RightMargin = 42;

            // 多筆資料依序列印
            foreach (PlanResultModel item in data )
            {
                Parameter.PROJECT_NO = item.PLAN_NO;
                object[] objData = new object[] { Parameter, Builder };
                await CreateService<RPTProjectList>(objData, "PlanExecutionSurvey").MakeContenByTemplate();
                /// 最後一次不加空白頁面
                if (item != data.Last())
                {
                    Builder.InsertBreak(BreakType.PageBreak);
                }
            }
            return true;
        }
    }
}
