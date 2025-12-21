using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IDownloadService
    {
        /// <summary>
        /// 下載範本檔
        /// </summary>
        /// <param name="tempFileName">範本檔名(含附檔名)</param>
        /// <param name="downFileName">下載結果檔名(不含附檔名)</param>
        /// <returns></returns>
        Task<(byte[] bytes, string fileName, string contentType)> GetTemplateFile(string tempFileName, string downFileName);
    }
}
