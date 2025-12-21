using Renci.SshNet;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using SDO.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class SFTPService : Service, IFTPService, IDisposable
    {
        private IConfiguration configuration;
        string userName = "";
        string token = "";
        string ftp = "";
        string host = "";
        string folder = "";
        private readonly SftpClient sftp;

        /// <summary>
        /// 是否自動斷線
        /// </summary>
        public bool AutoDisconnect { get; set; } = true;

        /// <summary>
        /// SFTP連接狀態
        /// </summary>
        public bool isConnected { get { return sftp.IsConnected; } }

        /// <summary>
        /// 設定 sftp 連線
        /// </summary>
        /// <param name="ip">sftp網址 IP</param>       
        /// <param name="user">使用者名稱</param>
        /// <param name="pwd">登入密碼 因 checkmarx掃描 命名簡化</param>
        /// <param name="port">預設22</param>
        public SFTPService(IConfiguration configuration)
        {
            this.configuration = configuration;
            userName = configuration.GetValue<string>("SftpSetting:userName");
            token = configuration.GetValue<string>("SftpSetting:password");
            ftp = configuration.GetValue<string>("SftpSetting:ftp");
            host = configuration.GetValue<string>("SftpSetting:host");
            folder = configuration.GetValue<string>("SftpSetting:folder");
            int? port = configuration.GetValue<int?>("SftpSetting:port");
            port = !port.HasValue ? 22 : port.Value;
            sftp = new SftpClient(host, port.Value, userName, token);
        }

        #region 連線SFTP
        /// <summary>
        /// 連線SFTP
        /// </summary>
        /// <returns>true成功</returns>
        public void Connect()
        {
            if (!isConnected)
            {
                sftp.Connect();
            }
        }
        #endregion

        #region 關閉SFTP
        /// <summary>
        /// 關閉SFTP
        /// </summary>
        public void Disconnect()
        {
            if (sftp != null && isConnected)
            {
                sftp.Disconnect();
            }
        }
        #endregion

        #region SFTP上傳檔案
        /// <summary>
        /// SFTP上傳檔案
        /// </summary>
        /// <param name="remotePath">遠端路徑</param>
        public void Upload(string remoteFullPath, byte[] fileContents)
        {
            remoteFullPath = remoteFullPath.Replace("\\", "/");
            var remotePath = Path.GetDirectoryName(remoteFullPath);
            remotePath = remotePath.Replace("\\", "/");
            if (!IsPathExsist(remotePath))
                CreateDirByDirNames(remotePath.Split('/'));
            using (MemoryStream ms = new MemoryStream(fileContents))
            {
                Connect();
                sftp.UploadFile(ms, GetRoot(remoteFullPath));
                if (AutoDisconnect) Disconnect();
            }
        }
        #endregion

        #region SFTP獲取檔案
        /// <summary>
        /// SFTP獲取檔案
        /// </summary>
        /// <param name="remotePath">遠端路徑</param>
        public byte[] Download(string remotePath)
        {
            remotePath = remotePath.Replace("\\", "/");
            Connect();
            var byt = sftp.ReadAllBytes(GetRoot(remotePath));
            if (AutoDisconnect) Disconnect();
            return byt;
        }
        #endregion

        #region 刪除SFTP檔案
        /// <summary>
        /// 刪除SFTP檔案
        /// </summary>
        /// <param name="remoteFile">遠端路徑</param>
        public void Delete(string remoteFile)
        {
            remoteFile = remoteFile.Replace("\\", "/");
            Connect();
            if (sftp.Exists(remoteFile))
                sftp.Delete(remoteFile);
            if (AutoDisconnect) Disconnect();
        }

        /// <summary>
        /// 刪除檔案(不用SFTP路徑目錄)
        /// </summary>
        /// <param name="remoteFile"></param>
        public void DeleteFile(string remoteFile)
        {
            remoteFile = remoteFile.Replace("\\", "/");
            Connect();
            string remoteFilefull = GetRoot(remoteFile);
            if (sftp.Exists(remoteFilefull))
                sftp.Delete(remoteFilefull);
            if (AutoDisconnect) Disconnect();
        }
        #endregion

        /// <summary>
        /// 建立資料夾
        /// </summary>
        /// <param name="Npath">路徑</param>
        /// <returns></returns>
        public bool CreateDir(string Npath)
        {
            Npath = Npath.Replace("\\", "/");
            string dirRoot = GetRoot(Npath);
            Connect();
            if (!sftp.Exists(dirRoot))
                sftp.CreateDirectory(dirRoot);
            if (AutoDisconnect) Disconnect();
            return true;
        }

        /// <summary>
        /// 刪除資料夾
        /// </summary>
        /// <param name="Npath">路徑</param>
        /// <returns></returns>
        public bool DeleteDir(string Npath)
        {
            Npath = Npath.Replace("\\", "/");
            string dirRoot = GetRoot(Npath);
            Connect();
            if (sftp.Exists(dirRoot))
            {
                // 先將資料夾裡面的檔案全部刪除
                IList<FileModel> fileList = sftp.ListDirectory(dirRoot)
                    .Where(x => x.IsRegularFile)
                    .Select(x => new FileModel { FullPath = x.FullName, ModifyDate = x.LastWriteTimeUtc }).ToList();
                foreach (FileModel file in fileList)
                {
                    sftp.Delete(file.FullPath);
                }
                // 再刪除資料夾本身
                sftp.DeleteDirectory(dirRoot);
            }
                
            if (AutoDisconnect) Disconnect();
            return true;
        }

        /// <summary>
        /// 取得SFTP檔案列表
        /// </summary>
        /// <param name="remotePath">遠端目錄</param>
        /// <returns></returns>
        public bool IsPathExsist(string remotePath)
        {
            remotePath = remotePath.Replace("\\", "/");
            Connect();
            bool IsExist = sftp.Exists(GetRoot(remotePath));
            if (AutoDisconnect) Disconnect();

            return IsExist;
        }

        /// <summary>
        /// 移動SFTP檔案
        /// </summary>
        /// <param name="oldRemotePath">舊遠端路徑</param>
        /// <param name="newRemotePath">新遠端路徑</param>
        public void Move(string oldRemotePath, string newRemotePath)
        {
            Connect();
            string oldRemotePathfull = GetRoot(oldRemotePath),
                newRemotePathfull = GetRoot(newRemotePath);
            sftp.RenameFile(oldRemotePathfull, newRemotePathfull);
            if (AutoDisconnect) Disconnect();
        }

        /// <summary>
        /// 複製SFTP資料夾
        /// </summary>
        /// <param name="oldRemotePath">舊遠端路徑</param>
        /// <param name="newRemotePath">新遠端路徑</param>
        public void CopyDir(string oldRemotePath, string newRemotePath)
        {
            Connect();
            //如果需自動斷線,則要等整個scope做完才斷線故另外設定
            bool defaltDisconnect = AutoDisconnect;
            AutoDisconnect =AutoDisconnect ? false:AutoDisconnect;
            string oldRemotePathfull = GetRoot(oldRemotePath),
                newRemotePathfull = GetRoot(newRemotePath);
            foreach (var f in sftp.ListDirectory(oldRemotePathfull).Where(x => x.IsRegularFile))
            {
                if (!sftp.Exists(newRemotePathfull))
                    CreateDirByDirNames(newRemotePath.Split('/'));
                using (MemoryStream ms = new(f.Attributes.GetBytes()))
                {
                    sftp.UploadFile(ms, $"{newRemotePathfull}/{f.Name}.{f.Attributes.Extensions}");
                }
            }
            //回復自動斷線設定
            AutoDisconnect = defaltDisconnect;
            if (AutoDisconnect) Disconnect();
        }

        public IList<FileModel> getFiles(string path)
        {
            path = path.Replace("\\", "/");
            Connect();
            var fileList = sftp.ListDirectory(folder + path).Where(x => x.IsRegularFile).Select(x => new FileModel
            {
                FileName = x.Name,
                Extension = Path.GetExtension(x.Name),
                FullPath = x.FullName,
                ModifyDate = x.LastWriteTimeUtc
            }).ToList();
            if (AutoDisconnect) Disconnect();
            return fileList;
        }

        public List<string> GetDirectories(string directoryName = "")
        {
            Connect();
            List<string> directories = sftp.ListDirectory(folder + directoryName).Select(x => x.Name).ToList();
            if (AutoDisconnect) Disconnect();
            return directories;
        }

        //利用多個資料夾名稱組路徑
        public void CreateDirByDirNames(string[] dirNames)
        {
            var fileRoot = "";
            foreach (var dirName in dirNames)
            {
                if (!string.IsNullOrWhiteSpace(dirName))
                {
                    fileRoot += $"/{dirName}";
                    if (!IsPathExsist(fileRoot))
                        CreateDir(fileRoot);
                }
            }
        }

        //組路徑
        private string GetRoot(string root)
        {
            root = root.Replace("\\", "/");
            return folder.TrimEnd('/') + "/" + root.TrimStart('/');
        }

        public void Dispose()
        {
            if (sftp != null && isConnected)
            {
                sftp.Disconnect();
            }
        }
    }
}

