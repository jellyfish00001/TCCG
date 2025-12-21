using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
namespace SDO.Services
{
    public class FTPsService: Service
    {

        string ftp = "ftp://{0}/{1}";
        string ftpaddr = "";
        FtpWebRequest request;
        /// <summary>
        /// 取FTPrequest物件
        /// </summary>
        /// <param name="FTPPath">ftp路徑</param>
        private void FTPSignIn(string FTPPath)
        {

            request = (FtpWebRequest)WebRequest.Create(FTPPath);
            request.Credentials = new NetworkCredential("username", "password");
         
            // support FTPS
            request.EnableSsl = true;
            request.UseBinary = true;
            request.KeepAlive = false;
            request.UsePassive = true;
        }

        /// <summary>
        /// 檢查ftp上是否有此資料夾
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public async Task<bool> CheckFolderExist(string dir)
        {
            string ftppath = string.Format(ftp, ftpaddr, dir);
            FTPSignIn(ftppath);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            using (WebResponse response = await request.GetResponseAsync())
            {
                return ((FtpWebResponse)response).StatusCode == FtpStatusCode.FileActionOK;
            }
        }
        /// <summary>
        /// ftp upload
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public async Task<bool>  Upload(string filepath,byte[] fileContents)
        {

            string ftppath = string.Format(ftp, ftpaddr, filepath) ;
            FTPSignIn(ftppath);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            using (Stream requestStream = request.GetRequestStream())
            {
               await requestStream.WriteAsync(fileContents, 0, fileContents.Length);
            }
            using (WebResponse response = await request.GetResponseAsync())
            {
                return ((FtpWebResponse)response).StatusCode == FtpStatusCode.FileActionOK;
            }
        }

        /// <summary>
        /// ftp download
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public async Task<byte[]> Download(string filepath)
        {
            string ftppath = string.Format(ftp, ftpaddr, filepath);
            FTPSignIn(ftppath);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            //如果要連線的 FTP 伺服器要求憑據並支援安全套接字層 (SSL)，則應將 EnableSsl 設定為 true。
            //如果不寫會報出421錯誤（服務不可用）
            request.EnableSsl = true;
            // 首次連線FTP server時，會有一個證書分配過程。
            //如果沒有下面的程式碼會報異常：
            //System.Security.Authentication.AuthenticationException: 根據驗證過程，遠端證書無效。
            ServicePointManager.ServerCertificateValidationCallback =
               new RemoteCertificateValidationCallback(ValidateServerCertificate);
            using (var res = await request.GetResponseAsync())
            {
                using (Stream stream = res.GetResponseStream())
                {
                    if (stream != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            stream.CopyTo(memoryStream);
                            return memoryStream.ToArray();
                        }
                    }
                    else
                        return null;
                }
            }
        }

        public bool CreateFolder()
        {

            return false;
        }

        public bool Delete(string filePath)
        {
            string ftppath = string.Format("ftp://{0}", "");
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftppath);
            request.Credentials = new NetworkCredential("username", "password");
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            request.EnableSsl = true;
            request.UseBinary = true;
            request.KeepAlive = false;
            request.UsePassive = true;
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                return response.StatusCode== FtpStatusCode.FileActionOK;
            }
        }

    
        private static bool ValidateServerCertificate
      (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
