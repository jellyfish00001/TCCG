using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class AnnouncementQryModel
    {
        /// <summary>
        /// 組織清單
        /// </summary>
        public string[] ORG_IDs { get; set; }
        /// <summary>
        /// 登入人員
        /// </summary>
        public string USER_ID { get; set; }
        /// <summary>
        /// 公告類別
        /// </summary>
        public string ANN_TYPE { get; set; }
    }
}
