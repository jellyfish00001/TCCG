using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using SDO.Dac;
using SDO.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SDO.Utils;
using SDO.CryptSet;
using Microsoft.Extensions.Configuration;

namespace SDO.Services
{
    public class UploadFileCacheService : UploadFileService, IUploadFileService
    {
        private readonly IUploadFileDac dac;
        private readonly IUserProfile userProfile;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly ISetParamService setParamService;
        private readonly IEncryptService encryptService;
        private readonly IDecryptService decryptService;
        private readonly ICache cache;
        public UploadFileCacheService(IUploadFileDac dac, IUserProfile userProfile,
            IWebHostEnvironment webHostEnvironment, ISetParamService setParamService, IEncryptService encryptService, IDecryptService decryptService, ICache cache) :
            base(dac, userProfile,   webHostEnvironment, setParamService)
        {
            this.dac = dac;
            this.userProfile = userProfile;
            this.webHostEnvironment = webHostEnvironment;
            this.setParamService = setParamService;
            this.encryptService = encryptService;
            this.decryptService = decryptService;
            this.cache = cache;
        }

        /// <summary>
        /// 上傳檔案
        /// </summary>
        /// <param name="uploader"></param>
        /// <returns>fileSeqNo</returns>
        public override async Task<RtnResultModel> Save(IList<IFormFile> uploader)
        {
            if (uploader != null)
            {
                //uploader內只會有一個
                foreach (IFormFile file in uploader)
                {
                    //檢驗FileSingnature
                    if (!ValidContentType(file.FileName, file.ContentType) || !VaildFileSingnature(file.OpenReadStream(), file.FileName))
                    {
                        return ChangeResult(false);
                    }
                    await AppendToCache(file.FileName, file.OpenReadStream());
                }
                return ChangeResult(true);
            }
            return ChangeResult(false);
        }

        /// <summary>
        /// 儲存上傳檔案到user資料夾
        /// </summary>
        /// <param name="fileNames"></param>
        /// <param name="savePath"></param>
        /// <returns></returns>
        public async override Task<Array> SaveUploads(UploadPathModel model)
        {
            ArrayList saveFiles = new ArrayList();
            if (model.fileNames != null)
            {
                model.savePath = string.IsNullOrWhiteSpace(model.savePath) ? await GetUploadFolderPath() : ValidFilePath(model.savePath);
                Directory.CreateDirectory(Path.Combine(model.savePath, userProfile.GetLoginUser().USER_ID));
                foreach (string strFileSeqNo in model.fileNames)
                {
                    if (strFileSeqNo.Length == 0) return saveFiles.ToArray(typeof(string));
                    int fileSeqNo = int.Parse(strFileSeqNo);

                    UploadFileModel uploadFile = await GetByFileSeqNo(fileSeqNo);

                    string tmpFilePath = await GetUploadFolderPath(uploadFile.FILE_SAVE_NAME);
                    if (File.Exists(tmpFilePath))
                    {
                        //移動檔案 由暫存位質移至使用者所屬資料夾 ex savePath/aaa.xxx to savePath/User/aaa.xxx
                        File.Move(tmpFilePath, Path.Combine(model.savePath, uploadFile.FILE_SAVE_PATH));
                        //回傳 FILE_SEQ_NO
                        saveFiles.Add(uploadFile.FILE_SEQ_NO.ToString());
                    }
                    else
                    {
                        if (uploadFile.IsExists(model.savePath))
                        {
                            //回傳 FILE_SEQ_NO
                            saveFiles.Add(uploadFile.FILE_SEQ_NO.ToString());
                        }
                    }
                }
            }
            // delete trash file
            foreach (FileInfo tmpFile in new DirectoryInfo(await GetUploadFolderPath()).GetFiles().Where(p => p.CreationTime < Now.AddDays(-1)))
            {
                tmpFile.Delete();
            }
            return saveFiles.ToArray(typeof(string));
        }

        /// <summary>
        /// 取得上傳檔案資訊
        /// </summary>
        /// <param name="fileNames"></param>
        /// <param name="savePath"></param>
        /// <returns></returns>
        public async Task<(byte[] bytes, string fileName, string contentType)> Download(string fileSeqNo, string Uid, string fileName)
        {
            string savePath = await GetUploadFolderPath();
            if (string.IsNullOrWhiteSpace(fileSeqNo))
                return (null, "", "");

            int SeqNo = int.Parse(fileSeqNo);
            UploadFileModel uploadFile = await GetByFileSeqNo(SeqNo);
            if (uploadFile.IsExists(savePath))
            {
                byte[] attachBytes = uploadFile.GetFile(savePath);
                if (!VaildFileSingnature(attachBytes, uploadFile.FILE_FULL_NAME))
                {
                    attachBytes = decryptService.AES256(attachBytes, true).decryptedBytes;
                }
                string fileType;
                new FileExtensionContentTypeProvider().TryGetContentType(uploadFile.FILE_FULL_NAME, out fileType);
                return (bytes: attachBytes, fileName: uploadFile.FILE_FULL_NAME, contentType: fileType);
            }
            return (null, "", "");
        }
        /// <summary>
        /// 由cache讀取檔案
        /// </summary>
        /// <param name="Uid">檔案識別碼</param>
        /// <param name="fileName">完整檔名</param>
        /// <returns></returns>
        private async Task<(byte[] bytes, string fileName, string contentType)> LoadCacheFile(string Uid, string fileName)
        {
            string key = Uid;
            byte[] bytes = await cache.GetCache(key);
            string fileType;
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out fileType);
            return (bytes, fileName, contentType: fileType);
        }
        /// <summary>
        /// 將檔案binary加密後存到cach
        /// </summary>
        /// <param name="Uid">檔案識別碼</param>
        /// <param name="content">檔案內容</param>
        private async Task AppendToCache(string Uid, Stream content)
        {
            byte[] Filebyte = new byte[content.Length];
            content.Read(Filebyte, 0, Filebyte.Length);
            Filebyte = encryptService.AES256(Filebyte, true).encryptedBytes;
            await cache.SetCache(Uid, Filebyte);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">fullpath</param>
        /// <param name="FileName">完整檔名</param>
        /// <returns></returns>
        protected override (byte[] bytes, string fileName, string contentType) GetFileResult(string path, string FileName)
        {
            return default;
        }

    }
}
