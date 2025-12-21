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
    public class PWSRPTService : Service, IPWSRPTService
    {
        private readonly IReportFactory rpt;

        public PWSRPTService(IReportFactory rpt)
        {
            this.rpt = rpt;
        }

        private async Task<RtnRptModel> CreateRPT(RptParameter param)
        {
            return await rpt.CreateRPT<PWSRPTService>(param);
        }

        /// <summary>
        /// 取審查結果彙整表
        /// </summary>
        /// <returns></returns>
        public async Task<RtnRptModel> RPTPWSReport(PWSReportModel model)
        {
            // 特定報表產出word
            ReportTypeEnum reportType = (model.RPT_ID == "RPTProjectList" || model.RPT_ID == "RPTProjectPolicyReview" || model.RPT_ID == "RPTProjectEntList")
                                        ? ReportTypeEnum.Word
                                        : ReportTypeEnum.Excel;
            // 組CreateRPT所需資料
            RptParameter param = new RptParameter
            {
                ReportId = model.RPT_ID,
                FileName = model.STATISTICS_NAME,
                ObjectModel = model,
                ReportType = reportType
            };

            return await CreateRPT(param);
        }


    }
}
