using Microsoft.AspNetCore.Http;
using SDO.Base.Utils.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace SDO.Models
{
    public class UploadFileModel
    {
        public int FILE_SEQ_NO { get; set; }

        public string USER_ID { get; set; }

        public string FILE_UID { get; set; }

        public string FILE_NAME { get; set; }

        public string FILE_EXTENSION { get; set; }

        public string CRT_USER { get; set; }

        public string CRT_IP { get; set; }

        public DateTime CRT_DATE { get; set; }

        /// <summary>
        /// FILE_NAME + FILE_EXTENSION
        /// </summary>
        public string FILE_FULL_NAME
        {
            get
            {
                return Path.GetFileName($"{FILE_NAME}{FILE_EXTENSION}");
            }
        }

        /// <summary>
        /// FILE_UID + FILE_EXTENSION
        /// </summary>
        public string FILE_SAVE_NAME
        {
            get
            {
                return Path.GetFileName($"{FILE_UID}{FILE_EXTENSION}");
            }
        }

        /// <summary>
        /// USER_ID/FILE_UID + FILE_EXTENSION
        /// </summary>
        public string FILE_SAVE_PATH
        {
            get
            {
                return Path.Combine(USER_ID, FILE_SAVE_NAME);
            }
        }

        public byte[] GetFile(string savePath)
        {
            if (!IsExists(savePath))
                throw new FileLoadException("檔案不存在");

            string filePath = Path.Combine(savePath, FILE_SAVE_PATH);
            return File.ReadAllBytes(filePath);
        }

        public bool IsExists(string savePath)
        {
            return File.Exists(Path.Combine(savePath, FILE_SAVE_PATH));
        }
    }
}
