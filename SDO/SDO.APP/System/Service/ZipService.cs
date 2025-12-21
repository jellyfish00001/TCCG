using Ionic.Zip;
using Microsoft.AspNetCore.StaticFiles;
using SDO.Base.Utils;
using SDO.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class ZipService : Service, IZipService
    {
        private readonly IFTPService ftpService;

        public ZipService(IFTPService ftpService)
        {
            this.ftpService = ftpService;
        }

        /// <summary>
        /// 下載ftp檔案並壓成壓縮檔
        /// </summary>
        /// <param name="listModel"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public (byte[] bytes, string fileName, string contentType) MakeZip(List<ProjectAttachmentModel> listModel, string title)
        {
            // listModel 沒資料，直接回傳 null
            if (!listModel.Any())
            {
                return (bytes: null, fileName: string.Empty, contentType: string.Empty);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (ZipFile zip = new ZipFile(Encoding.UTF8))
                {
                    foreach (ProjectAttachmentModel fileModel in listModel)
                    {
                        string extension = Path.GetExtension(fileModel.FILE_NAME);
                        byte[] fileBytes = ftpService.Download($"{fileModel.FILE_PATH}/{fileModel.IDENTITY_FIELD + extension}");
                        // 加到zip，entryName: 若要再包一層資料夾，需寫資料夾名稱，若沒寫資料夾就會全部包在同一層
                        // 配合檔案檢核規則，以FILE_KIND區分資料夾
                        zip.AddEntry($"{fileModel.NAME}/{fileModel.FILE_NAME}", fileBytes);
                    }

                    // 無檔案例外處理
                    if (zip.Count == 0)
                    {
                        zip.AddEntry(i18N.Message.R09, "");
                    }
                    zip.Save(ms);
                }
                // zip 檔名
                string fileName = string.Format("{0}.zip", title);
                new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string contentType);
                contentType ??= "application/octet-stream";
                return (ms.ToArray(), contentType, fileName);
            }
        }
    }
}
