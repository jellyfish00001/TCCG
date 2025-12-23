using Microsoft.Extensions.Logging;
using SDO.APP.RD.Models.Report;
using SDO.Base.RPT.Enums;
using SDO.Dac;
using SDO.ReportBuilder.Interface;
using SDO.ReportBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class RDRPTService : Service, IRDRPTService
    {
        private readonly IReportFactory rpt;

        public RDRPTService(IReportFactory rpt)
        {
            this.rpt = rpt;
        }

        private async Task<RtnRptModel> CreateRPT(RptParameter param)
        {
            return await rpt.CreateRPT<RDRPTService>(param);
        }

        /// <summary>
        /// 統計報表
        /// </summary>
        /// <param name="model">統計報表model</param>
        /// <returns></returns>
        public async Task<RtnRptModel> RDReport(ReportQueryModel model)
        {
            List<(int, string, ReportTypeEnum)> reports = new List<(int, string, ReportTypeEnum)>
            {
                new (1, "RPTRDSeasonReport", ReportTypeEnum.Excel),
                new (2, "RPTRDPlanResultUsageSituations", ReportTypeEnum.Excel),
                new (3, "RPTRDPlanExecution", ReportTypeEnum.Excel),
                new (4, "RPTRDParticipating", ReportTypeEnum.Excel),
                new (5, "RPTRDPlanExecutionSurvey", ReportTypeEnum.Word)
            };

            var report = reports.FirstOrDefault(x => x.Item1 == model.REPORT_ID);
            RptParameter param = new RptParameter
            {
                ReportId = report.Item2,
                ReportType = report.Item3,
                PROJECT_NO = model.PLAN_NO,
                ObjectModel = model,
                FileName = model.STATISTICS_NAME
            };
            return await CreateRPT(param);
        }
    }
}
