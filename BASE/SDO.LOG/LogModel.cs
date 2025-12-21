using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SDO.LOG.Models
{
    public class LogModel
    {
        /// <summary>
        /// log紀錄時間
        /// </summary>
        public DateTime LogTime { get; set; }

        /// <summary>
        /// URL
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// Conteroller
        /// </summary>
        public string Controller { get; set; }
        /// <summary>
        /// Action
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// 傳入參數
        /// </summary>
        public Dictionary<string,object> RequestBody { get; set; }
        /// <summary>
        /// 使用者帳號
        /// </summary>
        public string User_Id { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string User_Name { get; set; }
        /// <summary>
        /// ip
        /// </summary>
        public string User_Ip { get; set; }
        /// <summary>
        /// computer name
        /// </summary>
        public string User_Macheine { get; set; }
    }
}
