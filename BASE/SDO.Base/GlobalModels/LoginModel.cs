using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 經濟部登入Model
    /// </summary>
    public class LoginModel
    {
        //[Required]
        public virtual string USER_ID { get; set; }

        [Required]
        public string USER_PD { get; set; }
        [Required]
        public string Captcha { get; set; }
        public string CaptchaEncoded { get; set; }
        public bool isPass2Auth { get; set; }
        public string TOKEN { get; set; }
    }

    public class LoginExChangeModel
    {
        public string USER_ID { get; set; }

        public string USER_PD { get; set; }

        /// <summary>
        /// 密碼型態 A:加密 B:未加密
        /// </summary>
        public string USER_TYPE { get; set; }
    }
}
