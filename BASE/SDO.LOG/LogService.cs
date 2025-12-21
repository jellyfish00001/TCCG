using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using SDO.Services;
using SDO.LOG.Models;
namespace SDO.LOG.Services
{
    public class LogService : Service
    {
        private IWebHostEnvironment webHostEnvironment;


        public LogService(IWebHostEnvironment webHostEnvironment)
        {
            this.webHostEnvironment = webHostEnvironment;
        }
        /// <summary>
        /// 取得APILog
        /// </summary>
        /// <param name="UserId">使用者ID</param>
        /// <param name="LogDateS">起日</param>
        /// <param name="LogDateE">迄日</param>
        /// <returns></returns>
        public IList<LogModel> GetApiLog(string UserName, DateTime LogDateS, DateTime LogDateE)
        {
            IList<LogModel> logList = new List<LogModel>();
            var pathstructure = webHostEnvironment.ContentRootPath.Split('\\').ToList();
            pathstructure.Remove(pathstructure.LastOrDefault());
            string WEBFolder = string.Join("\\", pathstructure.ToArray());
            string path = @$"{WEBFolder}\logs\apiLog\";
            //string logFile = DateTime.Now.ToString("yyyyMMdd") + ".log";
            for (DateTime date = LogDateS; date <= LogDateE; date = date.AddDays(1))
            {
                string logFile = date.ToString("yyyyMMdd") + ".log";
                if (File.Exists(@$"{path}\{logFile}"))
                {
                    using (StreamReader sr = new StreamReader(@$"{path}\{logFile}"))
                    {
                        do
                        {
                            string s = sr.ReadLine();
                            var log = JsonDeserialize<LogModel>(s);
                            logList.Add(log);
                        } while (!sr.EndOfStream);
                    }
                }
            }
            return logList.Where(x => x.User_Name.Contains(UserName)).ToList();
        }

        public string GetErrorLog(DateTime LogDate)
        {
            string log = "";
            var pathstructure = webHostEnvironment.ContentRootPath.Split('\\').ToList();
            //pathstructure.Remove(pathstructure.LastOrDefault());
            string WEBFolder = string.Join("\\", pathstructure.ToArray());
            string path = @$"{WEBFolder}\logs\";
            string logFile = LogDate.ToString("yyyy-MM-dd") + ".log";
            if (File.Exists(@$"{path}\{logFile}"))
            {
                using (StreamReader sr = new StreamReader(@$"{path}\{logFile}"))
                {
                    log = sr.ReadToEnd();
                }
            }
            
            //if (string.IsNullOrEmpty(log))
            //{
            //    return rootPath;
            //}

            return log;
        }
    }
}
