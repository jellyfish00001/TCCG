using System.Collections.Generic;
using SDO.Models;
namespace SDO.Services
{
    public interface IFTPService
    {
        public bool AutoDisconnect { get; set; }
        void Upload(string Path, byte[] file);
        bool IsPathExsist(string remotePath);
        byte[] Download(string remotePath);
        void Delete(string remoteFile);
        bool CreateDir(string Npath);
        bool DeleteDir(string Npath);
        void Move(string oldRemotePath, string newRemotePath);
        void CopyDir(string oldRemotePath, string newRemotePath);
        IList<FileModel> getFiles(string path);
        List<string> GetDirectories(string directoryName = "");
        void CreateDirByDirNames(string[] dirNames);
        void DeleteFile(string remoteFile);
        void Connect();
        void Disconnect();
    }
}
