using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SDO.Attributes;
using SDO.Models;
using SDO.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [AuthorizeLogin]
    public class UploadFileController : ControllerBase
    {
        private readonly IUploadFileService uploadFileService;

        public UploadFileController(IUploadFileService uploadFileService)
        {
            this.uploadFileService = uploadFileService;
        }

        /// <summary>
        /// 上傳檔案
        /// </summary>
        /// <param name="uploader"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> UploadFile(IList<IFormFile> files)
        {
            return await uploadFileService.Save(files);
        }

        /// <summary>
        /// 儲存上傳檔案到user資料夾
        /// </summary>
        /// <param name="uploader"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<Array> SaveUploads(UploadPathModel model)
        {
            return await uploadFileService.SaveUploads(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uploader"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> RemoveUploads([FromForm] string fileSeqNo,[FromForm] bool isUploaded = false)
        {
            return await uploadFileService.Remove(fileSeqNo,isUploaded);
         }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uploader"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ArrayList> GetUploads(UploadPathModel model)
        {
            return await uploadFileService.GetUploads(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uploader"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<FileResult> Download([FromForm] string fileSeqNo, [FromForm] string fileName, [FromForm] string uid)
        {
            var rtn = await uploadFileService.Download(fileSeqNo);
            rtn.fileName = fileSeqNo == "" ? fileName : rtn.fileName;
            return File(rtn.bytes, rtn.contentType, fileName);
        }

    

        /// <summary>
        /// 上傳暫存檔案
        /// </summary>
        /// <param name="files">檔案</param>
        /// <returns>success:回傳 成功/失敗, message:Guid值(檔案識別碼)</returns>
        [HttpPost("[action]")]
        public RtnResultModel UploadTempFile([FromForm] List<IFormFile> files)
        {
            return uploadFileService.UploadTempFile(files[0]);
        }

        /// <summary>
        /// 移除暫存檔案
        /// </summary>
        /// <param name="guid">檔案識別碼</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel RemoveTempFile([FromBody]string uid)
        {
            return uploadFileService.RemoveTempFile(uid);
        }

        /// <summary>
        /// 移除昨天暫存檔案
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel RemoveYTDTempFile()
        {
            return uploadFileService.RemoveYTDTempFile();
        }
    }
}