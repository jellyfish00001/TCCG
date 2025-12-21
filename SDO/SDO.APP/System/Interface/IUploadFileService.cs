using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using SDO.Base.Utils.Models;
using SDO.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IUploadFileService
    {
        Task<UploadFileModel> GetByFileSeqNo(int fileSeqNo);

        Task<int> InsertUploadFile(UploadFileModel model);

        void DeleteUploadFile(string fileSeqNo);

        Task<RtnResultModel> Save(IList<IFormFile> uploader);

        Task<Array> SaveUploads(UploadPathModel model);

        Task<RtnResultModel> Remove([FromForm] string fileSeqNo, [FromForm] bool isUploaded = false);

        Task<ArrayList> GetUploads(UploadPathModel model);
        Task<(byte[] bytes, string fileName, string contentType)> Download(string fileSeqNo);
        Task<(byte[] bytes, string fileName, string contentType)> Download(string uid, string fileName, string fileSeqNo)
        {
            throw new NotFiniteNumberException();
        }

        /// <summary>
        /// 複製檔案
        /// </summary>
        /// <param name="fromFilePath">來源檔案路徑(含檔名)</param>
        /// <param name="destFilePath">目的檔案(含檔名)</param>
        void CopyFile(string fromFilePath, string destFilePath);

        /// <summary>
        /// 移動檔案
        /// </summary>
        /// <param name="SourceFilePath"></param>
        /// <param name="DestFilePath"></param>
        /// <returns></returns>
        bool MoveFile(string SourceFilePath, string DestFilePath);

        /// <summary>
        /// 上傳檔案
        /// </summary>
        /// <param name="file">檔案</param>
        /// <param name="model">上傳檔案資訊 Model</param>
        /// <returns></returns>
        RtnResultModel SaveFile(IFormFile file, SaveFileModel model);

        /// <summary>
        /// 刪除檔案
        /// </summary>
        /// <param name="filePath"></param>
        void DeleteFile(string filePath);

        /// <summary>
        /// 下載檔案
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task<(byte[] bytes, string fileName, string contentType)> GetDownloadFile(string filePath, string fileName);

        #region 暫存檔 (共用)
        /// <summary>
        /// 上傳暫存檔案 (共用)
        /// </summary>
        /// <param name="file">檔案</param>
        /// <returns></returns>
        RtnResultModel UploadTempFile(IFormFile file);

        /// <summary>
        /// 移除暫存檔案
        /// </summary>
        /// <param name="uid">檔案識別碼</param>
        /// <returns></returns>
        RtnResultModel RemoveTempFile(string uid);

        /// <summary>
        /// 移除昨天暫存檔案
        /// </summary>
        /// <returns></returns>
        RtnResultModel RemoveYTDTempFile();

        /// <summary>
        /// 儲存檔案
        /// </summary>
        /// <param name="models">暫存檔儲存 Model</param>
        /// <returns></returns>
        RtnResultModel SaveFile(List<SaveTempFileModel> models);
        #endregion 暫存檔 (共用)


		Task<bool> CheckMrgFile(string srno1, string planType, string planKind, string aplYear);
        FileExtensionContentTypeProvider GetCustomContentTypeProvider();
    }

}
