using SDO.Models;
using SDO.ReportBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IPWSRPTService
    {

        /// <summary>
        /// 審查結果彙整表
        /// </summary>
        /// <returns></returns>
        Task<RtnRptModel> RPTPWSReport(PWSReportModel model);

    } 
}
