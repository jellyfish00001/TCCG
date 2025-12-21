using SDO.APP.RD.Models.Report;
using SDO.ReportBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IRDRPTService
    {
        /// <summary>
        /// 報表列印
        /// </summary>
        /// <param name="model">報表列印 model</param>
        /// <returns></returns>
        Task<RtnRptModel> RDReport(ReportQueryModel model);
    }
}
