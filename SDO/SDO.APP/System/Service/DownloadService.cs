using Autofac;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using Microsoft.AspNetCore.StaticFiles;

namespace SDO.Services
{
    public class DownloadService : Service, IDownloadService
    {
        protected readonly IWebHostEnvironment webHostEnvironment;
        protected readonly ISetParamService sysParam;

        public DownloadService(IComponentContext context)
        {
            this.webHostEnvironment = context.Resolve<IWebHostEnvironment>();
            this.sysParam = context.Resolve<ISetParamService>();
        }

        /// <summary>
        /// 下載範本檔
        /// </summary>
        /// <param name="tempFileName">範本檔名(含附檔名)</param>
        /// <param name="downFileName">下載結果檔名(不含附檔名)</param>
        /// <returns></returns>
        public async Task<(byte[] bytes, string fileName, string contentType)> GetTemplateFile(string tempFileName, string downFileName)
        {
            string contentRootPath = webHostEnvironment.ContentRootPath;
            string downPath = (await sysParam.GetSysParam("SystemConfig", "ReportFolder")).SET_VALUE;
            downPath = downPath.TrimStart('~').TrimStart('\\');
            downPath = Path.Combine(contentRootPath, downPath, tempFileName);

            string downFileFullName = downFileName + Path.GetExtension(tempFileName);

            WebClient webClient = new WebClient();
            byte[] downByte = webClient.DownloadData(downPath);
            FileExtensionContentTypeProvider provider = new FileExtensionContentTypeProvider();
            // Add new mappings
            provider.Mappings[".odt"] = "application/vnd.oasis.opendocument.text";
            provider.TryGetContentType(downFileFullName, out string fileType);

            return (downByte, downFileFullName, fileType);

        }

    }
}
