using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class ProjectAttachmentQueryModel
    {
        /// <summary>
        /// 列管編號
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 01:相關檔案上傳、02:其他地方上傳
        /// </summary>
        public string FILE_UP_SOURCE { get; set; }
        /// <summary>
        /// SET_PARAM.SET_ITEM ='FILE_KIND'
        /// </summary>
        public List<string> FILE_KIND { get; set; }
        /// <summary>
        /// 上傳來源流水號
        /// </summary>
        public int? SOURCE_ID { get; set; }

        public int DB { get; set; }
    }
}
