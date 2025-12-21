using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class TokenModel : DbEditor
    {
        public int? TOKEN_ID { get; set; }
        public string TOKEN { get; set; }

        /// <summary>
        /// Token類型
        /// L-登入token A-API Token
        /// </summary>
        public string TOKEN_TYPE { get; set; }

        /// <summary>
        /// 使用者ID
        /// </summary>
        public string USER_ID { get; set; }

        /// <summary>
        /// Token到期日
        /// </summary>
        public DateTime EXPIRE_DATE { get; set; }
    }
}
