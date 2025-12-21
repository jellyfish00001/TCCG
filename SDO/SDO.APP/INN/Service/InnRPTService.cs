using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SDO.Base.RPT.Enums;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Interface;
using SDO.ReportBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class InnRPTService : Service, IInnRPTService
    {
        private readonly IReportFactory rpt;
        private readonly IInnProjectDac innProjectDac;
        private readonly IStatisticsService statisticsService;

        public InnRPTService(IReportFactory rpt, IInnProjectDac innProjectDac, IStatisticsService statisticsService)
        {
            this.rpt = rpt;
            this.innProjectDac = innProjectDac;
            this.statisticsService = statisticsService;
        }

        private async Task<RtnRptModel> CreateRPT(RptParameter param)
        {
            return await rpt.CreateRPT<InnRPTService>(param);
        }

        /// <summary>
        /// 取得各年度提案資料清冊
        /// </summary>
        /// <returns></returns>
        public async Task<RtnRptModel> RPTInnStatistics(InnStatisticsModel model)
        {
            RptParameter param = new RptParameter
            {
                ReportId = model.RPT_ID,
                FileName = model.STATISTICS_NAME,
                ObjectModel = model,
                ReportType = ReportTypeEnum.Excel
            };
            return await CreateRPT(param);
        }

        public async Task<RtnRptModel> ProjectPrint(InnProjectPrintQueryModel model)
        {

            string fileName = model.Year + model.FILE_NAME;
            RptParameter param = new RptParameter
            {
                ReportId = "RPTInnProjectPrint",
                FileName = fileName,
                ReportType = ReportTypeEnum.Word,
                YEAR = model.Year
            };

            if (model.INN_PLAN_NO != null)
            {
                param.PROJECT_NO_DATA.AddRange(model.INN_PLAN_NO);
            }

            return await CreateRPT(param);
        }



    }
}
