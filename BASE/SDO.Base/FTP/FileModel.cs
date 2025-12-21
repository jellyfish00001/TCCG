using System;
namespace SDO.Models
{
    public class FileModel
    {
        /// <summary>
        /// 完整路徑
        /// </summary>
        public string FullPath { get; set; }
        /// <summary>
        /// 檔名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 副檔名
        /// </summary>
        public string Extension { get; set; }
        /// <summary>
        /// 檔案修改日期
        /// </summary>
        public DateTime ModifyDate { get;set; }
    }
}
