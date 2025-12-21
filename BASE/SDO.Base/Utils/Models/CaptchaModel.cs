using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class CaptchaModel
    {
        /// <summary>
        /// 驗證碼
        /// </summary>
        public string Captcha { get; set; }
        /// <summary>
        /// 加密後驗證碼
        /// </summary>
        public string CaptchaEncode { get; set; }
        /// <summary>
        /// 驗證碼圖檔Base64
        /// </summary>
        public string Img { get; set; }
    }
}
