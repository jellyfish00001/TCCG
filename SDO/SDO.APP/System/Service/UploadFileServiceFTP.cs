using Microsoft.AspNetCore.Hosting;
using System.Transactions;
using Microsoft.AspNetCore.StaticFiles;
using SDO.Dac;
using SDO.Models;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SDO.Utils;

namespace SDO.Services
{
    public class UploadFileServiceFTP : UploadFileService, IUploadFileService
    {
        private readonly IUserProfile userProfile;
        private readonly ISetParamService setParamService;
        private readonly IFTPService ftpService;

        public UploadFileServiceFTP(IUploadFileDac dac, IUserProfile userProfile,  
            IWebHostEnvironment webHostEnvironment, ISetParamService setParamService, IFTPService ftpService) :
            base(dac, userProfile,   webHostEnvironment, setParamService)
        {
            this.userProfile = userProfile;
            this.setParamService = setParamService;
            this.ftpService = ftpService;
        }
        /// <summary>
        /// 取得檔案資訊
        /// </summary>
        /// <param name="files"></param>
        /// <param name="uploadFile"></param>
        /// <param name="path"></param>
        protected override void GetFiles(ArrayList files, UploadFileModel uploadFile, string path)
        {
            path = $"{path}/{uploadFile.FILE_SAVE_PATH}";
            if (ftpService.IsPathExsist(path))
            {
                byte[] attachBytes = ftpService.Download(path);
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
        /// 複製檔案
        /// </summary>
        /// <param name="fromFilePath">來源檔案路徑(含檔名)</param>
        /// <param name="destFilePath">目的檔案(含檔名)</param>
        public override void CopyFile(string fromFilePath, string destFilePath)
        {
            if (ftpService.IsPathExsist(Path.GetDirectoryName(fromFilePath.Replace("\\", "/"))))
            {
                // 檢查檔案的路徑是否存在，沒有建會建立資料夾
                CheckIsPathExsist(Path.GetDirectoryName(destFilePath.Replace("\\", "/")));
                byte[] fileByte = ftpService.Download(fromFilePath);
                Upload(destFilePath, fileByte);
            }
        }

        /// <summary>
        /// 刪除資料夾
        /// </summary>
        /// <param name="Path">資料夾路徑</param>
        /// <returns></returns>
        public override void DeleteDir(string Path)
        {
            ftpService.DeleteDir(Path);
        }

        /// <summary>
        /// 上傳檔案
        /// </summary>
        /// <param name="DestPath"></param>
        /// <param name="fileByte"></param>
        protected override void Upload(string DestPath, byte[] fileByte)
        {
            DestPath = ProcessPath(DestPath);
            //建立加密物件
            if (ftpService.IsPathExsist(DestPath))
            {
                ftpService.Delete(DestPath);
            }
            ftpService.Upload(DestPath, fileByte);
        }
        /// <summary>
        /// 刪除檔案
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        protected override void Delete(string Path)
        {
            if (ftpService.IsPathExsist(Path))
            {
                ftpService.Delete(Path);
            }
        }

        /// <summary>
        /// 移動檔案
        /// </summary>
        /// <param name="fromPath">來源路徑</param>
        /// <param name="toPath">目標路徑</param>
        protected override void Move(string fromPath, string toPath)
        {
            if (ftpService.IsPathExsist(toPath))
            {
                ftpService.Delete(toPath);
            }
            ftpService.Move(fromPath, toPath);
        }

        /// <summary>
        /// 刪除昨天暫存檔案
        /// </summary>
        protected override void DeleteYTDTemp()
        {
            foreach (var tmpFile in ftpService.getFiles("/").Where(p => p.ModifyDate < DateTime.UtcNow.AddDays(-1)))
            {
                ftpService.Delete(tmpFile.FullPath);
            }
        }

        /// <summary>
        /// 刪除指定暫存檔案
        /// </summary>
        /// <param name="uid">檔案識別碼</param>
        public override RtnResultModel RemoveTempFile(string uid)
        {
            foreach (FileModel tmpFile in ftpService.getFiles("/").Where(x => x.FullPath.Contains(uid)))
            {
                ftpService.Delete(tmpFile.FullPath);
            }
            return ChangeResult(true);
        }

        /// <summary>
        /// 取得檔案上傳資料夾路徑
        /// </summary>
        /// <returns></returns>
        protected override async Task<string> GetUploadFolderPath()
        {
            return (await setParamService.GetSysParam("SystemConfig", "UploadFolder")).SET_VALUE.Replace("~", "");
        }

        /// <summary>
        /// 儲存上傳檔案到資料夾
        /// </summary>
        /// <param name="fileNames"></param>
        /// <param name="savePath"></param>
        /// <returns></returns>
        public override async Task<Array> SaveUploads(UploadPathModel model)
        {
            ArrayList saveFiles = new ArrayList();
            string tempPath = await GetUploadFolderPath();
            if (model.fileNames != null)
            {
                model.savePath = string.IsNullOrWhiteSpace(model.savePath) ? tempPath : ValidFilePath(model.savePath);
                ftpService.CreateDir(Path.Combine(model.savePath, userProfile.GetLoginUser().USER_ID));
                foreach (string strFileSeqNo in model.fileNames)
                {
                    if (strFileSeqNo.Length == 0) return saveFiles.ToArray(typeof(string));
                    int fileSeqNo = int.Parse(strFileSeqNo);

                    UploadFileModel uploadFile = await GetByFileSeqNo(fileSeqNo);

                    string tmpFilePath = await GetUploadFolderPath(uploadFile.FILE_SAVE_NAME);
                    if (ftpService.IsPathExsist(tmpFilePath))
                    {
                        //移動檔案 由暫存位質移至使用者所屬資料夾 ex savePath/aaa.xxx to savePath/User/aaa.xxx
                        ftpService.Move(tmpFilePath, Path.Combine(model.savePath, uploadFile.FILE_SAVE_PATH));
                        //回傳 FILE_SEQ_NO
                        saveFiles.Add(uploadFile.FILE_SEQ_NO.ToString());
                    }
                    else
                    {
                        if (ftpService.IsPathExsist(model.savePath))
                        {
                            //回傳 FILE_SEQ_NO
                            saveFiles.Add(uploadFile.FILE_SEQ_NO.ToString());
                        }
                    }
                }
            }
            // delete trash file
            foreach (var tmpFile in ftpService.getFiles(tempPath).Where(p => p.ModifyDate < DateTime.UtcNow.AddDays(-1)))
            {
                ftpService.Delete(tmpFile.FullPath);
            }
            return saveFiles.ToArray(typeof(string));
        }
        protected override string Getfile(string RootPath, UploadFileModel uploadFile)
        {
            string savePath = Path.Combine(RootPath, $@"{uploadFile.FILE_SAVE_PATH}");
            //未存檔找temp目錄
            if (!ftpService.IsPathExsist(RootPath))
            {
                savePath = Path.Combine(RootPath, $@"{uploadFile.FILE_SAVE_NAME}");
                //找不到return null
                if (!ftpService.IsPathExsist(savePath))
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
        protected override (byte[] bytes, string fileName, string contentType) GetFileResult(string path, string FileName)
        {
            byte[] attachBytes = ftpService.Download(path);
            string fileType;
            FileExtensionContentTypeProvider provider = new FileExtensionContentTypeProvider();
            // Add new mappings
            provider.Mappings[".odt"] = "application/vnd.oasis.opendocument.text";
            provider.TryGetContentType(FileName, out fileType);
            return (bytes: attachBytes, fileName: FileName, contentType: fileType);
        }

        /// <summary>
        /// 檢查檔案路徑是否存在
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected  override bool isPathExsist(string path)
        {
            return ftpService.IsPathExsist(path);
        }
        /// <summary>
        /// 處理路徑分隔符號方向
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        protected override string ProcessPath(string filePath)
        {
            return filePath.Replace("\\", "/").Replace("//", "/");
        }
        /// <summary>
        /// 移動檔案
        /// </summary>
        /// <param name="SourceFilePath"></param>
        /// <param name="DestFilePath"></param>
        /// <returns></returns>
        public override bool MoveFile(string SourceFilePath, string DestFilePath)
        {
            if (!string.IsNullOrWhiteSpace(SourceFilePath) &&
                !string.IsNullOrWhiteSpace(DestFilePath) &&
                ftpService.IsPathExsist(SourceFilePath))
            {
                //取得上傳目錄
                var destFileRoot = Path.GetDirectoryName(DestFilePath).Replace("\\", "/");

                //建立SFTP上傳目錄
                if (!ftpService.IsPathExsist(destFileRoot))
                    ftpService.CreateDirByDirNames(destFileRoot.Split("/"));

                //如果目的地檔案已存在，要先砍掉，不然會報錯....
                if (ftpService.IsPathExsist(DestFilePath))
                    ftpService.DeleteFile(DestFilePath);

                //移動檔案
                ftpService.Move(SourceFilePath, DestFilePath);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 檢查資料夾是否沒有，沒有會建立資料夾
        /// </summary>
        /// <param name="filePath">存放的路徑</param>
        public override void CheckIsPathExsist(string filePath)
        {
            var dirNames = filePath.Replace("\\", "/").Split('/');
            ftpService.CreateDirByDirNames(dirNames);
        }

        /// <summary>
        /// 刪除FTP檔案
        /// </summary>
        /// <param name="filePath">檔案路徑</param>
        public override void DeleteFile(string filePath)
        {
            ftpService.DeleteFile(filePath);
        }

        /// <summary>
        /// 下載檔案
        /// </summary>
        /// <param name="filePath">路徑</param>
        /// <param name="fileName">檔名</param>
        /// <returns></returns>
        public override Task<(byte[] bytes, string fileName, string contentType)> GetDownloadFile(string filePath, string fileName)
        {
            //空檔
            var defaultRtn = GetFileResult(null, "","");

            //確認FTP上檔案是否存在
            if (!string.IsNullOrWhiteSpace(filePath) && ftpService.IsPathExsist(filePath))
                defaultRtn = GetFileResult(filePath, fileName);

            return Task.FromResult(defaultRtn);
        }
    }
}
