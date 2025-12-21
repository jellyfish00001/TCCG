using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 儲存上傳檔案Model
    /// </summary>
    public class UploadPathModel
    {
        public string[] fileNames { get; set; }
        public string savePath { get; set; }
    }
}
