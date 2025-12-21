using System;

namespace SDO.Models
{
    public class CaptchaVerifyModel : DbEditor
    {
        /// <summary>
        /// 使用者Id
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 驗證識別碼
        /// </summary>
        public string CaptchaId { get; set; }
        /// <summary>
        /// 驗證碼
        /// </summary>
        public string CaptchaCode { get; set; }
        /// <summary>
        /// 加密驗證碼
        /// </summary>
        public byte[] CaptchaCodeEncode { get; set; }
        /// <summary>
        /// 驗證碼過期時間
        /// </summary>
        public DateTime ExpiresAtTime { get; set; }
        /// <summary>
        /// 使用者WID
        /// </summary>
        public string WID { get; set; }

        /// <summary>
        /// 使用者Email
        /// </summary>
        public string USER_EMAIL { get; set; }

    }
}
