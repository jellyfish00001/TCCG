using System.IO;
using System.Threading.Tasks;
using SDO.ReportBuilder.Models;
namespace SDO.ReportBuilder.Interface
{
    public interface IRPTBuilder
    {
        /// <summary>
        /// 建立報表
        /// </summary>
        /// <param name="parameter">報表參數</param>
        /// <returns></returns>
        public abstract Task<(MemoryStream ms, string outputName, string mime)> Create(RptParameter parameter);
    }
}
