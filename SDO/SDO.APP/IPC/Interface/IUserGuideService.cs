using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IUserGuideService
    {
        /// <summary>
        /// 取得操作手冊清單
        /// </summary>
        /// <returns></returns>
        List<UserGuideModel> GetUserGuides();

        /// <summary>
        /// 下載操作手冊
        /// </summary>
        /// <param name="groupName">群組名稱</param>
        /// <param name="fileName">檔案名稱</param>
        /// <returns></returns>
        (byte[] bytes, string contentType, string fileName) DownloadFile(string groupName, string fileName);
    }
}
