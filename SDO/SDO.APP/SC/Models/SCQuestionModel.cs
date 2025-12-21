using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 忘記密碼
    /// </summary>
    public class SCQuestionModel
    {
        /// <summary>
        /// 帳號
        /// </summary>
        public string USER_ID { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string USER_NAME { get; set; }

        /// <summary>
        /// 電子郵件位址
        /// </summary>
        public string USER_EMAIL { get; set; }
    }
}
