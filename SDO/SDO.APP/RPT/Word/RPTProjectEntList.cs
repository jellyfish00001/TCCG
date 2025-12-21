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
    public class RPTProjectEntList : WordBuilder
    {
        /// <summary>
        /// 先期審查-委託研究計畫先期審查計畫表(管考+機關)
        /// </summary>
        private readonly IPWSRPTDac pwsRptDac;
        public RPTProjectEntList(IComponentContext coms) : base(coms)
        {
            pwsRptDac = coms.Resolve<IPWSRPTDac>();
        }

        protected override async Task<bool> MakeContent()
        {
            PWSReportModel model = (PWSReportModel)Parameter.ObjectModel;
            //抓取符合條件的PLAN_NO
            // 2:委託研究計畫
            model.PLANKIND = "2";
            List<string> data;
            if (model.PLANNOList != null && model.PLANNOList.Any())
            {
                data = model.PLANNOList;
            }
            else
            {
                data = await pwsRptDac.GetPlanReviewNOList(model);
            }
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
            foreach (string item in data)
            {
                Parameter.PROJECT_NO = item;
                object[] objData = new object[] { Parameter, Builder };
                await CreateService<RPTProjectList>(objData, "ProjectEntList").MakeContenByTemplate();
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
