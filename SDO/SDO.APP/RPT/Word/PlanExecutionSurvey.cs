using Aspose.Words;
using Aspose.Words.Tables;
using Autofac;
using SDO.Base.RPT.Enums;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Services;
using SDO.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using SDO.Utils;
using System.Threading.Tasks;
using SDO.ReportBuilder.Models;
using Aspose.Cells.Drawing;
using SDO.ReportBuilder.Interface;
using SDO.APP.RD.Models.Report;
using System.Reflection;

namespace SDO.APP.RPT.Word
{
    /// <summary>
    /// 續列管委託研究計畫成果及運用情形調查表
    /// </summary>
    public class PlanExecutionSurvey : WContentBuilder
    {
        private IRDReportDacDac statisticsDac;
        private ReportQueryModel statistics;
        private PlanExecutionSurveyModel data;

        public PlanExecutionSurvey(IComponentContext coms) : base(coms)
        {
            statisticsDac = coms.Resolve<IRDReportDacDac>();
            TemplateFileName = "RDPlanExecutionSurveyRPT.doc";
        }

        protected override async Task<Task> GetData()
        {
            ReportQueryModel model = (ReportQueryModel)Parameter.ObjectModel;
            statistics = model;
            model.PLAN_NO = Parameter.PROJECT_NO;
            data = await statisticsDac.GetPlanExecutionSurvery(model);
            BasicData = new
            {
                OU_NAME = data.OU_NAME,
                PLAN_NAME = data.PLAN_NAME,
                ENTRUST_UNIT_NAME = data.ENTRUST_UNIT_NAME,
                RESEARCH_NAME = data.RESEARCH_NAME,
                PLAN_DATE = data.PLAN_START_DATE.ToTwDateString("yyy年MM月") + "/" + data.PLAN_END_DATE.ToTwDateString("yyy年MM月"),
                SUM_MONEY = data.SUM_MONEY != 0 ? $"{data.SUM_MONEY:N0}千元" : null,
                SITUATION_TYPE = data.SITUATION_TYPE,
                PLAN_CAUSE = data.PLAN_CAUSE,
                PLAN_EXPECTED = data.PLAN_EXPECTED,
                SITUATION_DESC = data.SITUATION_DESC,
                CRT_DATE = data.CRT_DATE.ToTwDateString("yyy年MM月dd日")

            };
            return Task.CompletedTask;
        }

    }
}
