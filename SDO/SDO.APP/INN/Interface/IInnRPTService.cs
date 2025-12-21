using SDO.Models;
using SDO.ReportBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IInnRPTService
    {
        Task<RtnRptModel> ProjectPrint(InnProjectPrintQueryModel model);

        /// <summary>
        /// 取得各年度提案資料清冊
        /// </summary>
        /// <returns></returns>
        Task<RtnRptModel> RPTInnStatistics(InnStatisticsModel model);

    } 
}
