using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class UserGuideModel
    {
        /// <summary>
        /// 群組名稱
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 檔案清單
        /// </summary>
        public List<string> Files { get; set; }
    }
}
