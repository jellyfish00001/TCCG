using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Services;
namespace SDO.ReportBuilder.Interface
{
    public interface IReportFactory
    {
        /// <summary>
        /// 建立報表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        Task<RtnRptModel> CreateRPT<T>(RptParameter parameter);

        /// <summary>
        /// 建立報表並存檔
        /// </summary>
        /// <param name="parameter">參數檔</param>
        /// <returns></returns>
        Task CreateRPTBySave<T>(RptParameter parameter);

        /// <summary>
        /// 製作合併檔
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        Task<RtnRptModel> MergePDF(RptParameter parameter);
    }
}
