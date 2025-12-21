using Microsoft.AspNetCore.StaticFiles;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class UserGuideService : Service, IUserGuideService
    {
        private readonly IFTPService ftpService;
        private readonly string path = "UserGuide";

        public UserGuideService(IFTPService ftpService)
        {
            this.ftpService = ftpService;
        }

        /// <summary>
        /// 取得操作手冊清單
        /// </summary>
        /// <returns></returns>
        public List<UserGuideModel> GetUserGuides()
        {
            if (!ftpService.IsPathExsist(path))
            {
                return new List<UserGuideModel>();
            }

            List<UserGuideModel> result = ftpService.GetDirectories(path).Select(directory => new UserGuideModel
            {
                GroupName = directory,
                Files = ftpService.getFiles(@$"{path}\{directory}").Select(x => x.FileName).ToList()
            }).ToList();
            return result;
        }

        /// <summary>
        /// 下載操作手冊
        /// </summary>
        /// <param name="groupName">群組名稱</param>
        /// <param name="fileName">檔案名稱</param>
        /// <returns></returns>
        public (byte[] bytes, string contentType, string fileName) DownloadFile(string groupName, string fileName)
        {
            if (string.IsNullOrEmpty(groupName) || string.IsNullOrEmpty(fileName))
            {
                return (null, string.Empty, fileName);
            }

            byte[] bytes = ftpService.Download(Path.Combine(path, groupName, fileName));
            string contentType;
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType);
            return (bytes, contentType, fileName);
        }
    }
}
