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
using SDO.Base.Utils.Models;
using System.Transactions;
using Aspose.Pdf.Facades;

namespace SDO.Services
{
    public abstract class UploadFileService : Service, IUploadFileService
    {
        private readonly IUploadFileDac dac;
        private readonly IUserProfile userProfile;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly ISetParamService setParamService;
        //private readonly ICache cache;

        public UploadFileService(IUploadFileDac dac, IUserProfile userProfile, 
            IWebHostEnvironment webHostEnvironment, ISetParamService setParamService
          )
        {
            this.dac = dac;
            this.userProfile = userProfile;
            this.webHostEnvironment = webHostEnvironment;
            this.setParamService = setParamService;
        }
        /// <summary>
        /// 查詢上傳檔案
        /// </summary>
        /// <param name="fileSeqNo"></param>
        /// <returns></returns>
        public async Task<UploadFileModel> GetByFileSeqNo(int fileSeqNo)
        {
            return await dac.GetByFileSeqNo(fileSeqNo);
        }

        /// <summary>
        /// 新增上傳檔案
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<int> InsertUploadFile(UploadFileModel model)
        {
            return await dac.Insert(model);
        }

        /// <summary>
        /// 刪除UPLOAD_FILE_DATA 檔案資料
        /// </summary>
        /// <param name="fileSeqNo"></param>
        public void DeleteUploadFile(string fileSeqNo)
        {
            dac.Delete(fileSeqNo);
        }

        /// <summary>
        /// 上傳檔案
        /// </summary>
        /// <param name="uploader"></param>
        /// <returns>fileSeqNo</returns>
        public virtual async Task<RtnResultModel> Save(IList<IFormFile> uploader)
        {
            if (uploader != null)
            {
                int fileSeqNo = 0;
                //uploader內只會有一個
                foreach (IFormFile file in uploader)
                {
                    //檢驗FileSingnature
                    if (!ValidateFile(file))
                    {
                        return ChangeResult(false);
                    }

                    fileSeqNo = await AppendToFile(file.FileName, file.OpenReadStream());
                }
                return ChangeResult(true, fileSeqNo.ToString());
            }
            return ChangeResult(false);
        }

        /// <summary>
        /// 上傳暫存檔
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileName"></param>
        /// <param name="fileCount"></param>
        private RtnResultModel UploadTempFile(IFormFile file, string fileName, int fileCount)
        {
            // 檢驗FileSingnature
            if (!ValidateFile(file))
                return ChangeResult(false);

            // 取得bytes
            byte[] fileByte = new byte[file.OpenReadStream().Length];
            file.OpenReadStream().Read(fileByte, 0, fileByte.Length);

            // 寫入FTP
            Upload(fileName, fileByte);
            return ChangeResult(true, fileCount.ToString());
        }

        /// <summary>
        /// 複製檔案
        /// </summary>
        /// <param name="fromFilePath">來源檔案路徑(不含檔名)</param>
        /// <param name="destFilePath">目的檔案(不含檔名)</param>
        public virtual void CopyFile(string fromFilePath, string destFilePath)
        {

        }

        /// <summary>
        /// 刪除資料夾
        /// </summary>
        /// <param name="Path">資料夾路徑</param>
        /// <returns></returns>
        public virtual void DeleteDir(string Path)
        {
            
        }

        /// <summary>
        /// 儲存上傳檔案到user資料夾
        /// </summary>
        /// <param name="fileNames"></param>
        /// <param name="savePath"></param>
        /// <returns></returns>
        public virtual async Task<Array> SaveUploads(UploadPathModel model)
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
            DeleteYTDTemp();
            return saveFiles.ToArray(typeof(string));
        }

        /// <summary>
        /// 移除檔案
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> Remove(string fileSeqNo, bool isUploaded = false)
        {
            if (!string.IsNullOrEmpty(fileSeqNo))
            {
                // isUploaded 用來判斷 是要刪除暫存目錄還是正確目錄
                string deletePath = "";
                UploadFileModel uploadFile = await GetByFileSeqNo(int.Parse(fileSeqNo));
                if (isUploaded)
                    deletePath = await GetUploadFolderPath(uploadFile.FILE_SAVE_PATH);
                else
                    deletePath = await GetUploadFolderPath(uploadFile.FILE_SAVE_NAME);

                // 從DB刪除資料 (待補上)
                dac.Delete(fileSeqNo);
                Delete(deletePath);
            }
            return ChangeResult(ResultType.Success | ResultType.Delete);
        }

        /// <summary>
        /// 刪除檔案
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        protected virtual void Delete(string Path)
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }

        /// <summary>
        /// 移動檔案
        /// </summary>
        /// <param name="fromPath">來源路徑</param>
        /// <param name="toPath">目標路徑</param>
        protected virtual void Move(string fromPath, string toPath)
        {

        }

        /// <summary>
        /// 刪除昨天暫存檔案
        /// </summary>
        protected virtual async void DeleteYTDTemp()
        {
            var path = await GetUploadFolderPath();
            // delete trash file
            foreach (FileInfo tmpFile in new DirectoryInfo(path).GetFiles().Where(p => p.CreationTime < Now.AddDays(-1)))
            {
                tmpFile.Delete();
            }
        }

        /// <summary>
        /// 刪除指定暫存檔案
        /// </summary>
        /// <param name="guid">檔案識別碼</param>
        protected virtual void DeleteTemp(string guid)
        {

        }

        /// <summary>
        /// 取得上傳檔案資訊
        /// </summary>
        /// <param name="fileNames"></param>
        /// <param name="savePath"></param>
        /// <returns></returns>
        public async Task<ArrayList> GetUploads(UploadPathModel model)
        {
            ArrayList files = new ArrayList();
            model.savePath = string.IsNullOrWhiteSpace(model.savePath) ? await GetUploadFolderPath() : ValidFilePath(model.savePath);
            if (model.fileNames != null)
            {
                foreach (string strFileSeqNo in model.fileNames)
                {
                    if (string.IsNullOrWhiteSpace(strFileSeqNo))
                        continue;

                    int fileSeqNo = int.Parse(strFileSeqNo);
                    UploadFileModel uploadFile = await GetByFileSeqNo(fileSeqNo);
                    GetFiles(files, uploadFile, model.savePath);
                }
            }
            return files;
        }

        /// <summary>
        /// 取得檔案資訊
        /// </summary>
        /// <param name="files"></param>
        /// <param name="uploadFile"></param>
        /// <param name="path"></param>
        protected virtual void GetFiles(ArrayList files, UploadFileModel uploadFile, string path)
        {
            if (uploadFile.IsExists(path))
            {
                byte[] attachBytes = uploadFile.GetFile(path);
                files.Add(new
                {
                    name = $"{uploadFile.FILE_FULL_NAME}",
                    size = attachBytes.Length,
                    extension = uploadFile.FILE_EXTENSION,
                    uid = $"{uploadFile.FILE_UID}",
                    fileSeqNo = $"{uploadFile.FILE_SEQ_NO}",
                    progress = 100, //Kendo React Upload 處理進度
                    status = 4 //Kendo React Upload 檔案狀態(4:Uploaded上傳成功)
                });
            }
        }

        /// <summary>
        /// 取得上傳檔案資訊
        /// </summary>
        /// <param name="fileNames"></param>
        /// <param name="savePath"></param>
        /// <returns></returns>
        public async Task<(byte[] bytes, string fileName, string contentType)> Download(string fileSeqNo)
        {
            string RootPath = await GetUploadFolderPath();
            if (string.IsNullOrWhiteSpace(fileSeqNo))
                return (null, "", "");

            int SeqNo = int.Parse(fileSeqNo);
            UploadFileModel uploadFile = await GetByFileSeqNo(SeqNo);
            string savePath = Path.Combine(RootPath, $@"{uploadFile.FILE_SAVE_PATH}");
            if (savePath == "")
                return (null, "", "");
            return GetFileResult(savePath, uploadFile.FILE_FULL_NAME);
        }
        protected virtual string Getfile(string RootPath, UploadFileModel uploadFile)
        {
            string savePath = Path.Combine(RootPath, $@"{uploadFile.FILE_SAVE_PATH}");
            //未存檔找temp目錄
            if (!uploadFile.IsExists(RootPath))
            {
                savePath = Path.Combine(RootPath, $@"{uploadFile.FILE_SAVE_NAME}");
                //找不到return null
                if (!File.Exists(savePath))
                    return "";
            }
            return savePath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">fullpath</param>
        /// <param name="FileName">完整檔名</param>
        /// <returns></returns>
        protected abstract (byte[] bytes, string fileName, string contentType) GetFileResult(string path, string FileName);

        protected virtual (byte[] bytes, string fileName, string contentType) GetFileResult(byte[] bytes, string fileName, string contentType)
        {
            return (bytes: bytes, fileName: fileName, contentType: contentType);
        }


        /// <summary>
        /// 驗證PDF檔案是否有保護密碼(沒有才能上傳)
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="extensionName"></param>
        /// <returns></returns>
        protected bool ValidatePdfProtectedPWD(IFormFile file)
        {
            if (file!=null && Path.GetExtension(file.FileName).ToLower().Replace(".", string.Empty) == "pdf")
            {
                PdfFileInfo fileInfo = new PdfFileInfo(file.OpenReadStream());
                if (file == null)
                {
                    return false;
                }
                return !fileInfo.IsEncrypted;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        protected string ValidFilePath(string fileName)
        {
            return fileName.Replace(@"../", "").Replace(@"..\", "").Replace(@"..", "");
        }

        public FileExtensionContentTypeProvider GetCustomContentTypeProvider()
        {
            //複寫odt ods 的Type
            var provider = new FileExtensionContentTypeProvider();
            var mimes = provider.Mappings;
            mimes.Add(".odt", "application/vnd.oasis.opendocument.text");
            if (mimes.ContainsKey(".ods"))
                mimes[".ods"] = "application/vnd.oasis.opendocument.spreadsheet";

            return new FileExtensionContentTypeProvider(mimes);
        }

        /// <summary>
        /// 檢查ContentType與檔案類型是否相符
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        protected bool ValidContentType(string fileName, string contentType)
        {
            string fileType;
            //複寫odt ods 的Type
            //var provider = new FileExtensionContentTypeProvider();
            //var mimes = provider.Mappings;
            //mimes.Add(".odt", "application/vnd.oasis.opendocument.text");
            //if (mimes.ContainsKey(".ods"))
            //    mimes[".ods"] = "application/vnd.oasis.opendocument.spreadsheet";
            var provider = GetCustomContentTypeProvider();
            //new FileExtensionContentTypeProvider(mimes).TryGetContentType(fileName, out fileType);
            provider.TryGetContentType(fileName, out fileType);
            if (fileType.Equals(contentType))
                return true;
            return false;
        }

        /// <summary>
        /// 檢驗FileSingnature(文件簽名)
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="fileName"></param>
        /// <param name="allowedChars">允許字元設定(for 文字檔)</param>
        /// <returns></returns>
        protected bool VaildFileSingnature(Stream fileStream, string fileName)
        {
            //取得副檔名
            string extensionName = Path.GetExtension(fileName).ToUpper();

            //取得FileSingnature列表
            IList<byte[]> fileSingnatures = GetFileSingnature(extensionName);

            //無對應FileSingnature不檢查(過濾檔案類型)
            if (fileSingnatures == null || !fileSingnatures.Any())
                return false;

            byte[] fileHexCode = new byte[fileStream.Length];
            fileStream.Read(fileHexCode, 0, fileHexCode.Length);
            //還原Position至開頭
            fileStream.Seek(0, SeekOrigin.Begin);

            //檢驗檔案開頭HexCode是否合法
            foreach (byte[] fileSingnature in fileSingnatures)
            {
                if (fileHexCode.Length < fileSingnature.Length)
                    return false;

                if (fileHexCode.Take(fileSingnature.Length).SequenceEqual(fileSingnature))
                    return true;
            }

            return false;
        }

        protected bool VaildFileSingnature(byte[] fileByte, string fileName)
        {
            //取得副檔名
            string extensionName = Path.GetExtension(fileName).ToUpper();

            //取得FileSingnature列表
            IList<byte[]> fileSingnatures = GetFileSingnature(extensionName);

            //無對應FileSingnature不檢查(過濾檔案類型)
            if (fileSingnatures == null || !fileSingnatures.Any())
                return false;

            byte[] fileHexCode = fileByte;
            //檢驗檔案開頭HexCode是否合法
            foreach (byte[] fileSingnature in fileSingnatures)
            {
                if (fileHexCode.Length < fileSingnature.Length)
                    return false;

                if (fileHexCode.Take(fileSingnature.Length).SequenceEqual(fileSingnature))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 寫入檔案
        /// </summary>
        /// <param name="tmpFilePath"></param>
        /// <param name="content"></param>
        protected virtual async Task<int> AppendToFile(string fileName, Stream content)
        {
            //取得bytes
            byte[] fileByte = new byte[content.Length];
            content.Read(fileByte, 0, fileByte.Length);
            //建立加密物件
            dac.BeginTransaction();
            (int fileSeqNo, string uid) = await SetUploadDataFile(fileName);
            //更改檔名儲存
            fileName = $"{uid}{Path.GetExtension(fileName)}";

            fileName = ValidFilePath(fileName);//CheckMarx want it!
            string tmpFilePath = await GetUploadFolderPath(fileName);

            Upload(tmpFilePath, fileByte);

            dac.Commit();
            return fileSeqNo;
        }

        protected virtual void Upload(string DestPath, byte[] fileByte)
        {
           var FilePath= ValidFilePath(DestPath);
            //建立加密物件
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
            //寫入檔案
            File.WriteAllBytes(FilePath, fileByte);
        }

        /// <summary>
        /// 新增檔案上傳資料
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        protected async Task<(int fileSeqNo, string uid)> SetUploadDataFile(string fileName)
        {
            string uid = Guid.NewGuid().ToString();
            int fileSeqNo;
            UploadFileModel uploadFile = new UploadFileModel()
            {
                USER_ID = userProfile.GetLoginUser().USER_ID,
                FILE_UID = uid,
                FILE_NAME = Path.GetFileNameWithoutExtension(fileName),
                FILE_EXTENSION = Path.GetExtension(fileName),
                CRT_USER = userProfile.GetLoginUser().USER_ID,
                CRT_IP = userProfile.GetLoginUser().USER_IP
            };

            fileSeqNo = await InsertUploadFile(uploadFile);

            return (fileSeqNo, uid);
        }

        /// <summary>
        /// 取得檔案上傳資料夾路徑
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<string> GetUploadFolderPath()
        {
            return (await setParamService.GetSysParam("SystemConfig", "UploadFolder")).SET_VALUE.Replace("~", webHostEnvironment.ContentRootPath);
        }

        /// <summary>
        /// 取得檔案上傳資料夾路徑並加上檔名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        protected async Task<string> GetUploadFolderPath(string fileName)
        {
            return Path.Combine(await GetUploadFolderPath(), fileName);
        }

        /// <summary>
        /// 取得檔案類型對應的 Hex Code
        /// 資料來源：https://en.wikipedia.org/wiki/List_of_file_signatures
        /// </summary>
        /// <param name="extensionName"></param>
        /// <returns></returns>
        protected IList<byte[]> GetFileSingnature(string extensionName)
        {
            switch (extensionName.ToUpper())
            {
                case ".PPT":
                case ".XLS":
                case ".DOC":
                    return new List<byte[]> { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } };
                case ".XLSX":
                case ".PPTX":
                case ".DOCX":
                    return new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 } };
                case ".PDF":
                    return new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 } };
                case ".ZIP":
                    return new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                                              new byte[] { 0x50, 0x4B, 0x4C, 0x49, 0x54, 0x55 },
                                              new byte[] { 0x50, 0x4B, 0x53, 0x70, 0x58 },
                                              new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                                              new byte[] { 0x50, 0x4B, 0x07, 0x08 },
                                              new byte[] { 0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70 } };
                case ".PNG":
                    return new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } };
                case ".JPG":
                    return new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                                              new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                                              new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 } };
                case ".JPEG":
                    return new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                                              new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                                              new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 } };
                case ".7Z":
                    return new List<byte[]> { new byte[] { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C } };
                case ".RAR":
                    return new List<byte[]> { new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00 },
                                              new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x01, 0x00 }};
                case ".ODS":
                case ".ODT":
                    return new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                                              new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                                              new byte[] { 0x50, 0x4B, 0x07, 0x08 } };
                default:
                    return null;
            }
        }
        /// <summary>
        /// 驗證檔案是否合法
        /// </summary>
        /// <param name="IFormFile">檔案物件</param>
        /// <returns></returns>
        protected bool ValidateFile(IFormFile file)
        {
            if (!ValidContentType(file.FileName, file.ContentType) || !VaildFileSingnature(file.OpenReadStream(), file.FileName))
                return false;
            else
                return true;
        }
        /// <summary>
        /// 處理路徑分隔符號方向
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        protected virtual string ProcessPath(string filePath)
        {
            return filePath.Replace("/", "\\");
        }
        /// <summary>
        /// 移動檔案
        /// </summary>
        /// <param name="SourceFilePath"></param>
        /// <param name="DestFilePath"></param>
        /// <returns></returns>
        public virtual bool MoveFile(string SourceFilePath, string DestFilePath)
        {
            return false;
        }

        /// <summary>
        /// 上傳檔案
        /// </summary>
        /// <param name="file">檔案</param>
        /// <param name="model">上傳檔案資訊 Model</param>
        /// <returns></returns>
        public virtual RtnResultModel SaveFile(IFormFile file, SaveFileModel model)
        {
            model.SavePath = ValidFilePath(model.SavePath);
            // 檢驗FileSingnature
            if (!ValidateFile(file))
            {
                return ChangeResult(false);
            }

            // 檢查資料夾是否沒有，沒有會建立資料夾
            CheckIsPathExsist(model.SavePath);

            // 刪除圖檔
            if (!string.IsNullOrEmpty(model.DeleteFileName))
            {
                string deleteFilePath = ValidFilePath(Path.Combine(model.SavePath, model.DeleteFileName));
                DeleteFile(deleteFilePath);
            }

            // 取得檔名
            string fileName = string.IsNullOrEmpty(model.NewFileName) ? file.FileName : model.NewFileName;

            // 取得存取路徑含檔名
            string filePath = Path.Combine(model.SavePath, fileName);

            // 取得bytes
            byte[] fileByte = new byte[file.OpenReadStream().Length];
            file.OpenReadStream().Read(fileByte, 0, fileByte.Length);

            // 寫入FTP
            Upload(filePath, fileByte);
            return ChangeResult(true);
        }

        /// <summary>
        /// 檢查資料夾是否沒有，沒有會建立資料夾
        /// </summary>
        /// <param name="filePath">檔案存檔位置，不含檔名</param>
        public virtual void CheckIsPathExsist(string savePath)
        {
            savePath = ValidFilePath(savePath);
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);
        }
        /// <summary>
        /// 檢查檔案路徑是否存在
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected virtual bool isPathExsist(string path)
        {
           var  Npath = ValidFilePath(path);
            return File.Exists(Npath);
        }

        /// <summary>
        /// 刪除FTP檔案
        /// </summary>
        /// <param name="filePath">檔案路徑</param>
        public virtual void DeleteFile(string filePath)
        {
            var Npath = ValidFilePath(filePath);
            if (File.Exists(Npath))
                File.Delete(Npath);
        }

        /// <summary>
        /// 下載檔案
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public virtual Task<(byte[] bytes, string fileName, string contentType)> GetDownloadFile(string filePath, string fileName)
        {
            return null;
        }

        #region 暫存檔 (共用)
        /// <summary>
        /// 上傳暫存檔案 (共用)
        /// </summary>
        /// <param name="file">檔案</param>
        /// <returns></returns>
        public RtnResultModel UploadTempFile(IFormFile file)
        {
            string extension = Path.GetExtension(file.FileName).ToUpper();
            string guid = Guid.NewGuid().ToString();
            string newFileName = $"{guid}{extension}";

            // 檢驗FileSingnature
            if (!ValidateFile(file))
                return ChangeResult(false);

            //驗證PDF檔案是否有保護密碼
            if (!ValidatePdfProtectedPWD(file))
                return ChangeResult(false, "PDF檔案請勿設定加密或保全");

            // 取得bytes
            byte[] fileByte = new byte[file.OpenReadStream().Length];
            file.OpenReadStream().Read(fileByte, 0, fileByte.Length);

            // 寫入FTP
            Upload(newFileName, fileByte);
            return ChangeResult(true, guid);
        }

        /// <summary>
        /// 移除暫存檔案
        /// </summary>
        /// <param name="uid">檔案識別碼</param>
        /// <returns></returns>
        public virtual RtnResultModel RemoveTempFile(string uid)
        {
            foreach (var tmpFile in Directory.GetFiles("\\").Where(x => x.Contains(uid)).ToList())
            {
                var Npath = ValidFilePath(tmpFile);
                File.Delete(Npath);
            }
            return ChangeResult(true);
        }

        /// <summary>
        /// 移除昨天暫存檔案
        /// </summary>
        /// <returns></returns>
        public RtnResultModel RemoveYTDTempFile()
        {
            DeleteYTDTemp();
            return ChangeResult(true);
        }

        /// <summary>
        /// 儲存檔案
        /// </summary>
        /// <param name="models">暫存檔儲存 Model</param>
        /// <returns></returns>
        public RtnResultModel SaveFile(List<SaveTempFileModel> models)
        {
            if (models == null)
            {
                return ChangeResult(false);
            }

            // 檢查檔案的路徑是否存在
            List<string> paths = models.Select(x => x.FormalPath).Distinct().ToList();
            foreach (var path in paths)
            {
                CheckIsPathExsist(path);
            }

            // 將暫存檔案移動至正式位置
            foreach (var item in models)
            {
                Move(item.TempFileName, Path.Combine(item.FormalPath, item.FormalFileName));
            }
            return ChangeResult(true);
        }
        #endregion 暫存檔 (共用)

        /// <summary>
        /// 檢查計畫合併檔是否存在
        /// </summary>
        /// <param name="srno1"></param>
        /// <param name="planType"></param>
        /// <param name="planKind"></param>
        /// <param name="aplYear"></param>
        /// <returns></returns>
        public virtual async Task<bool> CheckMrgFile(string srno1, string planType, string planKind, string aplYear) => await Task.FromResult(false);



    }
}
