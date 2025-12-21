using Aspose.Cells;
using SDO.ReportBuilder.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using static SDO.Models.AsposeExcelSetModel;

namespace SDO.Services
{
    public interface IAsposeExcelSetService
    {
        string SetContentType(string Format);

        /// <summary>
        /// 縱向動態長欄位(Auto create Row)
        /// </summary>
        /// <param name="saveFormat"></param>
        /// <returns></returns>
        Task<byte[]> ReportAuto(string saveFormat);

        /// <summary>
        /// 縱向動態長欄位(預設列數)
        /// </summary>
        /// <param name="saveFormat"></param>
        /// <returns></returns>
        Task<byte[]> ReportDefault(string saveFormat);

        /// <summary>
        /// 單一欄位套版
        /// </summary>
        /// <param name="saveFormat"></param>
        /// <returns></returns>
        Task<byte[]> ReportSingle(string saveFormat);

        Task<byte[]> GetSampleFile();

        Task<RtnRptModel> ReportAutoRPT(string saveFormat);
    }
}
