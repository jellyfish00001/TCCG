using Autofac;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Services;
using SDO.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SDO.ReportBuilder.Models;

namespace SDO.APP.RPT.Word
{
    public class ProjectPolicyReview : WContentBuilder
    {
        private IPWSRPTDac pwsRptDac;
        private IFundingExecutionService fundingExecutionService;
        private PWSReportModel pwsRptModel;
        private RPTProjectReviewList data;

        public ProjectPolicyReview(IComponentContext coms) : base(coms)
        {
            pwsRptDac = coms.Resolve<IPWSRPTDac>();
            fundingExecutionService = coms.Resolve<IFundingExecutionService>();
            TemplateFileName = "ProjectPolicyReview.doc";
        }

        // 抓資料和表單資料
        protected override async Task<Task> GetData()
        {
            PWSReportModel model = (PWSReportModel)Parameter.ObjectModel;
            pwsRptModel = model;
            model.PLANNO = Parameter.PROJECT_NO;
            // 計畫資料
            data = await pwsRptDac.GetPlanReviewList(pwsRptModel);
            // 轉換年度字串
            int planYear = Convert.ToInt16(pwsRptModel.PWS_YEAR);
            // 經費明細+執行情形
            FundingExecutionModel fundingExecutionData = await fundingExecutionService.GetFundingExecution(model.PLANNO, planYear);

            // 資料文字替換
            BasicData = new
            {
                // 計畫資料
                YEAR = pwsRptModel.PWS_YEAR,
                PLANNAME = data.PLANNAME,
                PLANORDERNUMBER = $"{data.PLANORDERNUMBER}",
                CREATE_ORG_UNIT = $"{data.ORGOUNAME} ({data.UNITOUNAME})",
                PLANTOTMONEY = $"{string.Format("{0:#,###}", data.PLANTOTMONEY)}千元",
                PLANDATE = $"{data.PLANSTARTDATE.ToTwDateString("yyy年M月d日")} ~ {data.PLANENDDATE.ToTwDateString("yyy年M月d日")}",
                PUBLICMONEY = $"{string.Format("{0:#,###}", data.PUBLICMONEY)}千元",
                FUNDMONEY = $"{string.Format("{0:#,###}", data.FUNDMONEY)}千元",
                PUBLIC_ADD_FUND = $"{string.Format("{0:#,###}", (data.PUBLICMONEY + data.FUNDMONEY))}千元",
                TOTAL = $"{string.Format("{0:#,###}", data.PLANTOTMONEY)}千元",
                // 勾選框
                C1 = data.PLANDATETYPE == "3" ? "■" : "□",
                C2 = data.PLANDATETYPE == "2" ? "■" : "□",
                C3 = data.PLANDATETYPE == "1" ? "■" : "□",
                C4 = data.PLANORGINYN == "Y" ? "■" : "□",
                C5 = data.PLANORGINYN == "N" ? "■" : "□",
                C6 = data.PLANCONTENTENGINE == "1" ? "■" : "□",
                C7 = data.PLANCONTENTLAND == "1" ? "■" : "□",
                C8 = data.PLANCONTENTINFO == "1" ? "■" : "□",
                C9 = data.BUDGETTYPE == "1" ? "■" : "□",
                C10 = data.BUDGETTYPE == "2" ? "■" : "□",
                // 延續性計畫歷年預算執行情形
                BYEAR = Convert.ToInt16(pwsRptModel.PWS_YEAR) - 1,
                BBYEAR = Convert.ToInt16(pwsRptModel.PWS_YEAR) - 2,
                BBBYEAR = Convert.ToInt16(pwsRptModel.PWS_YEAR) - 3,
                PUBLICMONEY1 = $"{string.Format("{0:#,###}", fundingExecutionData.budgetExecListModel[0].PUBLICMONEY)}千元",
                PUBLICMONEY2 = $"{string.Format("{0:#,###}", fundingExecutionData.budgetExecListModel[1].PUBLICMONEY)}千元",
                PUBLICMONEY3 = $"{string.Format("{0:#,###}", fundingExecutionData.budgetExecListModel[2].PUBLICMONEY)}千元",
                GROWRATIO1 = $"{fundingExecutionData.budgetExecListModel[0].GROWRATIO}%",
                GROWRATIO2 = $"{fundingExecutionData.budgetExecListModel[1].GROWRATIO}%",
                EXECOUNT1 = $"{string.Format("{0:#,###}", fundingExecutionData.budgetExecListModel[0].EXECOUNT)}千元",
                EXECOUNT2 = $"{string.Format("{0:#,###}", fundingExecutionData.budgetExecListModel[1].EXECOUNT)}千元",
                EXECOUNT3 = $"{string.Format("{0:#,###}", fundingExecutionData.budgetExecListModel[2].EXECOUNT)}千元",
                RATIO1 = $"{fundingExecutionData.budgetExecListModel[0].RATIO}%",
                RATIO2 = $"{fundingExecutionData.budgetExecListModel[1].RATIO}%",
                RATIO3 = $"{fundingExecutionData.budgetExecListModel[2].RATIO}%",
                EXEDESC1 = fundingExecutionData.budgetExecListModel[0].EXEDESC,
                EXEDESC2 = fundingExecutionData.budgetExecListModel[1].EXEDESC,
                EXEDESC3 = fundingExecutionData.budgetExecListModel[2].EXEDESC,
                average = $"{(fundingExecutionData.budgetExecListModel[1].RATIO + fundingExecutionData.budgetExecListModel[2].RATIO)/2}%"
            };

            // 經費需求迴圈
            ListData = new List<ITableData>();
            List<object> listData = new();
            if (fundingExecutionData.DAMTBListModel.Count != 0)
            {
                foreach (DAMTBModel item in fundingExecutionData.DAMTBListModel)
                {
                    listData.Add(new
                    {
                        FUNDDESC = item.FUNDDESC,
                        FUNDTOT = $"{string.Format("{0:#,###}", item.FUNDTOT)}千元",
                        CALCULATIONDESC = item.CALCULATIONDESC,
                    });
                }
                ListData.Add(new WordTableData { LIST_DATA = listData, TABLE_INDEX = 1});
            }

            return Task.CompletedTask;
        }

    }
}
