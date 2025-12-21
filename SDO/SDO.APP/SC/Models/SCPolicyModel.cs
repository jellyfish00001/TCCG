using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 密碼規則
    /// </summary>
    public class SCPolicyModel
    {
        /// <summary>
        /// 原則代碼
        /// </summary>
        public string POLICY_ID { get; set; }

        /// <summary>
        /// 密碼最短長度限制
        /// </summary>
        public int PASS_MIN_LEN { get; set; }

        /// <summary>
        /// 密碼不可與使用者代號或使用者名稱相同
        /// </summary>
        public string PASS_NO_SAME_ID_NAME { get; set; }

        /// <summary>
        /// 密碼4項至少需符合3項(大寫英文字母,小寫英文字母,數字,特殊符號)
        /// </summary>
        public string PASS_MIX_CHAR_NUM { get; set; }

        /// <summary>
        /// 不可含空白或特殊字元，只能用文字 A-Z 或數字 1-9 組成
        /// </summary>
        public string PASS_NO_SPEC_CHAR { get; set; }

        /// <summary>
        /// 至少含 {0} 個特殊字元
        /// </summary>
        public string PASS_AT_LEAST_SPECIAL_CHARS { get; set; }

        /// <summary>
        /// 相鄰二字元不可相同
        /// </summary>
        public string PASS_NO_SAME_2 { get; set; }

        /// <summary>
        /// 相鄰三字元不可為連續升冪或降冪
        /// </summary>
        public string PASS_NO_CONT_3 { get; set; }

        /// <summary>
        /// 不可與最近 {0} 次內重複
        /// </summary>
        public int PASS_NO_SAME_PAST_TIMES { get; set; }

        /// <summary>
        /// 每 {0} 天需變更密碼
        /// </summary>
        public int PASS_CHANGE_IN_DAYS { get; set; }

    }
}
