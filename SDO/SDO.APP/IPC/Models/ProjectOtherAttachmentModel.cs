using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class ProjectOtherAttachmentModel: ProjectAttachmentModel
    {
        /// <summary>
        /// 區分檔案類型
        /// </summary>
        public string FILE_TYPE { get; set; }
    }
}
