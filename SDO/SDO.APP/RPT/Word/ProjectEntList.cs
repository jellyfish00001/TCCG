using Aspose.Pdf;
using Aspose.Words;
using Aspose.Words.Tables;
using Autofac;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.APP.IPC.Models.Statistics;
using SDO.Base.RPT.Enums;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Services;
using SDO.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Table = Aspose.Words.Tables.Table;

namespace SDO.APP.RPT.Word
{
    public class ProjectEntList : WContentBuilder
    {
        private IPWSRPTDac pwsRptDac;
        private IFundingExecutionService fundingExecutionService;
        private PWSReportModel pwsRptModel;
        private RPTProjectEntListModel data;

        /// <summary>
        /// 取TempFile檔案
        /// </summary>
        /// <param name="coms"></param>
        public ProjectEntList(IComponentContext coms) : base(coms)
        {
            pwsRptDac = coms.Resolve<IPWSRPTDac>();
            fundingExecutionService = coms.Resolve<IFundingExecutionService>();
            TemplateFileName = "ProjectEntList.doc";
        }

        /// <summary>
        /// 取得表單資料
        /// </summary>
        /// <returns></returns>
        protected override async Task<Task> GetData()
        {
            PWSReportModel model = (PWSReportModel)Parameter.ObjectModel;
            pwsRptModel = model;
            model.PLANNO = Parameter.PROJECT_NO;
            // 計畫資料
            data = await pwsRptDac.GetEntPlanReviewList(pwsRptModel);
            // 轉換年度字串
            int planYear = Convert.ToInt16(pwsRptModel.PWS_YEAR);
            // 經費明細+執行情形
            FundingExecutionModel fundingExecutionData = await fundingExecutionService.GetFundingExecution(model.PLANNO, Convert.ToInt16(pwsRptModel.PWS_YEAR));
            // 計算總經費
            int TOTAL = 0;
            // 經費需求迴圈
            ListData = new List<ITableData>();
            List<object> listData = new();
            if (fundingExecutionData.DAMTBListModel.Count != 0)
            {
                foreach (DAMTBModel item in fundingExecutionData.DAMTBListModel)
                {
                    listData.Add(new
                    {
                        Ind = item.FUNDID,
                        FUNDDESC = item.FUNDDESC,
                        CALCULATIONDESC = item.CALCULATIONDESC,
                        PRICE = $"{string.Format("{0:#,###}", item.PRICE)}千元",
                        AMOUNT = item.AMOUNT,
                        FUNDTOT = $"{string.Format("{0:#,###}", item.FUNDTOT)}千元"
                    });
                    TOTAL += item.FUNDTOT;
                }

                ListData.Add(new WordTableData { LIST_DATA = listData, TABLE_INDEX = 1 });
            }
            // 歷年執行情形
            List<object> listData2 = new();
            if (fundingExecutionData.budgetExecListModel.Count != 0)
            {
                foreach (BudgetExecModel item in fundingExecutionData.budgetExecListModel)
                {
                    listData2.Add(new
                    {
                        EXEYEAR = item.EXEYEAR,
                        PUBLICMONEY2 = $"{string.Format("{0:#,###}", item.PUBLICMONEY)}千元",
                        GROWRATIO = item.GROWRATIO,
                        EXECOUNT = $"{string.Format("{0:#,###}", item.EXECOUNT)}千元",
                        RATIO = item.RATIO,
                        EXEDESC = item.EXEDESC
                    });
                }

                ListData.Add(new WordTableData { LIST_DATA = listData2, TABLE_INDEX = 1 });
            }
            // 中央預算處理
            string Y_CENTERMONEY = "";
            string N_CENTERMONEY = "";
            if (data.APPROVEDYN == "Y")
            {
                Y_CENTERMONEY = $"{string.Format("{0:#,###}", data.CENTERMONEY)}千元";
            }
            else
            {
                N_CENTERMONEY = $"{string.Format("{0:#,###}", data.CENTERMONEY)}千元";
            }

            // 資料文字替換
            BasicData = new
            {
                // 計畫資料
                TODAY = DateTime.Now.ToTwDateString(),
                PLANNAME = data.PLANNAME,
                OU_ID = data.OU_NAME,
                PLANORDERNUMBER = data.PLANORDERNUMBER,
                PLANDATE = $"{data.PLANSTARTDATE.ToTwDateString()} ~ {data.PLANENDDATE.ToTwDateString()}",
                AWARDYM = data.AWARDYM.ToTwDateString(),
                MIDREPORTYM = data.MIDREPORTYM.ToTwDateString(),
                FINAKREPORTYM = data.FINAKREPORTYM.ToTwDateString(),
                CLOSEYM = data.CLOSEYM.ToTwDateString(),
                PLANDATETYPE = data.PLANDATETYPE == "1" ? "單一年度計畫" : "跨年度計畫",
                PUBLICMONEY = $"{string.Format("{0:#,###}", data.PUBLICMONEY)}",
                FUNDMONEY = $"{string.Format("{0:#,###}", data.FUNDMONEY)}",
                OTHERMONEY = $"{string.Format("{0:#,###}", data.OTHERMONEY)}",
                Y_CENTERMONEY = Y_CENTERMONEY,
                APPROVEDNUMBER = data.APPROVEDNUMBER,
                N_CENTERMONEY = N_CENTERMONEY,
                PLANTOTMONEY = $"{string.Format("{0:#,###}", data.PLANTOTMONEY)}",
                LASTYEAR = Convert.ToInt16(pwsRptModel.PWS_YEAR) - 1,
                YEAR = pwsRptModel.PWS_YEAR,
                NEXTYEAR = Convert.ToInt16(pwsRptModel.PWS_YEAR) + 1,
                BYMoney = $"{string.Format("{0:#,###}", data.BYMoney)}",
                NYMoney = $"{string.Format("{0:#,###}", data.NYMoney)}",
                AYMoney = $"{string.Format("{0:#,###}", data.AYMoney)}",
                PLANCAUSE = data.PLANCAUSE,
                PLANEXPECTED = data.PLANEXPECTED,
                TOTAL = $"{string.Format("{0:#,###}", TOTAL)}千元",
                // 勾選框
                C1 = data.LABORYN == "Y" ?
                "■ 是 □ 否，非本府委託研究計畫認定範圍" :
                "□ 是 ■ 否，非本府委託研究計畫認定範圍",
                C2 = data.PUBLICMONEY != 0 ? "■" : "□",
                C3 = data.FUNDMONEY != 0 ? "■" : "□",
                C4 = data.OTHERMONEY != 0 ? "■" : "□",
                C5 = data.CENTERMONEY != 0 ? "■" : "□",
                C6 = data.APPROVEDYN == "Y" ? "■" : "□",
                C7 = data.APPROVEDYN == "N" ? "■" : "□",
                C8 = data.APPLYAPPROVEDYN == "Y" ? "■" : "□",
                C9 = data.APPLYAPPROVEDYN == "Y" ? "■" : "□",
                C10 = data.PLANTOTMONEY != 0 ? "■" : "□",
                C11 = data.BYMoney != 0 ? "■" : "□",
                C12 = data.NYMoney != 0 ? "■" : "□",
                C13 = data.AYMoney != 0 ? "■" : "□",
                C14 = fundingExecutionData.budgetExecListModel[0].NOBUDGETYN == 1 ? "■ 無" : "□ 無",
            };
            return Task.CompletedTask;
        }

    }
}
