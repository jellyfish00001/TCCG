using Aspose.Words;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SDO.Utils;
namespace SDO.Services
{
    public class ExportWordReportService : Service, IExportWordReportService
    {
        private readonly IWordSetService wordSetService;
        private readonly ISysParam sysParam;
        private readonly IUserProfile userProfile;
        private readonly IWebHostEnvironment webHostEnvironment;
       
        public ExportWordReportService(IWordSetService wordSetService, ISysParam sysParam, IUserProfile userProfile, IWebHostEnvironment webHostEnvironment)
        {
            this.wordSetService = wordSetService;
            this.sysParam = sysParam;
            this.userProfile = userProfile;
            this.webHostEnvironment = webHostEnvironment;
        }

       

        public async Task<(byte[] bytes, string fileName, string contentType)> ExportWord(string saveFormat)
        {
            UserDataModel userData = (UserDataModel)userProfile.GetLoginUser();

            IList<SetParamItemModel> objs = await sysParam.GetSysParamItems();

            Enum.TryParse(saveFormat, true, out SaveFormat wordSaveFormat);
            if (wordSaveFormat == SaveFormat.Unknown)
            {
                wordSaveFormat = SaveFormat.Docx;
                saveFormat = "docx";
            }

            string tmpFilePath = Path.Combine(await GetTmpDirPath(), "測試匯出報表.docx");

            using MemoryStream ms = wordSetService.GenWord(userData, objs, tmpFilePath, 1, wordSaveFormat, 1);
            string fileName = "WordExportExample." + saveFormat;
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string contentType);
            contentType ??= "application/octet-stream";
            return (bytes: ms.ToArray(), fileName: fileName, contentType: contentType);
        }

        public async Task<(byte[] bytes, string fileName, string contentType)> DownloadDescriptionFile()
        {
            string fileName = "SDOWord套表元件說明.doc";
            string tmpFilePath = Path.Combine(await GetTmpDirPath(), fileName);
            byte[] bytes = await File.ReadAllBytesAsync(tmpFilePath);
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string contentType);
            contentType ??= "application/octet-stream";

            return (bytes: bytes, fileName: fileName, contentType: contentType);
        }

        private async Task<string> GetTmpDirPath()
        {
            string contentRootPath = webHostEnvironment.ContentRootPath;
            string tmpFilePath = (await sysParam.GetSysParam("SystemConfig", "ReportFolder")).SET_VALUE;
            tmpFilePath = tmpFilePath.TrimStart('~').TrimStart('\\'); //Path.Combine 開頭不可為"\"
            return Path.Combine(contentRootPath, tmpFilePath);
        }
    }
}
