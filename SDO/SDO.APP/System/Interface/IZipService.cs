using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SDO.Models;

namespace SDO.Services
{
    public interface IZipService
    {
        /// <summary>
        /// 下載ftpService上檔案並壓成壓縮檔
        /// </summary>
        /// <param name="fileList"></param>
        /// <param name="zipTitle"></param>
        /// <returns></returns>
        (byte[] bytes, string fileName, string contentType) MakeZip(List<ProjectAttachmentModel> fileList, string zipTitle);
    }
}
