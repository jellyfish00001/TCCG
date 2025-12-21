using System.IO;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IExportWordReportService
    {
        Task<(byte[] bytes, string fileName, string contentType)> DownloadDescriptionFile();
        Task<(byte[] bytes, string fileName, string contentType)> ExportWord(string saveFormat);
    }
}