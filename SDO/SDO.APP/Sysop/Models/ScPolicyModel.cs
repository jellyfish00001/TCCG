using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SDO.Models
{
    public class ScPolicyModel : DbEditor
    {
        /// <summary>
        /// 原則代號
        /// </summary>
        public string POLICY_ID { get; set; }

        /// <summary>
        /// 原則代號
        /// </summary>
        public string POLICY_COMP_ID { get; set; }

        /// <summary>
        /// 帳號最短長度限制
        /// </summary>
        [DisplayName("帳號最短長度限制")]
        [RegularExpression(@"^[\d]*$", ErrorMessage = "請輸入正整數")]
        [Range(1, 50, ErrorMessage = "帳號長度必須介於1~50")]
        [Required(ErrorMessage = "必須設定帳號長度")]
        public int ID_MIN_LEN { get; set; }

        /// <summary>
        /// 密碼最短長度限制
        /// </summary>
        [DisplayName("密碼最短長度限制")]
        [RegularExpression(@"^[\d]*$", ErrorMessage = "請輸入正整數")]
        [Range(0, 50, ErrorMessage = "密碼長度必須介於1~50")]
        [Required(ErrorMessage = "必須設定密碼長度")]
        public int PASS_MIN_LEN { get; set; }

        /// <summary>
        /// 密碼不可與使用者代號或使用者名稱相同
        /// Y、N
        /// </summary>
        public char PASS_NO_SAME_ID_NAME { get; set; }

        /// <summary>
        /// PASS_NO_SPEC_CHAR
        /// Y、N
        /// </summary>
        public char PASS_NO_SPEC_CHAR { get; set; }

        /// <summary>
        /// 密碼必須為文數字混合
        /// Y、N
        /// </summary>
        public char PASS_MIX_CHAR_NUM { get; set; }

        /// <summary>
        /// PASS_NO_SAME_2
        /// Y、N
        /// </summary>
        public char PASS_NO_SAME_2 { get; set; }

        /// <summary>
        /// PASS_NO_CONT_3
        /// Y、N
        /// </summary>
        public char PASS_NO_CONT_3 { get; set; }

        /// <summary>
        /// PASS_NO_SAME_PAST_TIMES
        /// </summary>
        [DisplayName("密碼變更限制")]
        [RegularExpression(@"^[\d]*$", ErrorMessage = "請輸入正整數")]
        [Range(0, 20, ErrorMessage = "次數必須介於0~20")]
        [Required(ErrorMessage = "必須輸入次數")]
        public int PASS_NO_SAME_PASS_TIMES { get; set; }

        /// <summary>
        /// PASS_AT_LEAST_SPECIAL_CHARS
        /// </summary>
        public int PASS_AT_LEAST_SPECIAL_CHARS { get; set; }

        /// <summary>
        /// 帳戶不關閉
        /// </summary>
        public string ID_NO_CLOSE { get; set; }

        /// <summary>
        /// 同一天密碼連續錯誤次數，超過次數，則設定無效（帳戶鎖定）
        /// </summary>
        [DisplayName("同一天密碼連續錯誤次數限制")]
        [RegularExpression(@"^[\d]*$", ErrorMessage = "請輸入正整數")]
        [Range(0, 20, ErrorMessage = "次數必須介於0~20之間")]
        [Required(ErrorMessage = "必須輸入次數")]
        public int ID_DISABLE_IN_CONT { get; set; }

        /// <summary>
        /// 同一天密碼連續錯誤超過限制之帳號鎖定時間限制
        /// </summary>
        [DisplayName("同一天密碼連續錯誤超過限制之帳號鎖定時間限制")]
        [RegularExpression(@"^[\d]*$", ErrorMessage = "請輸入正整數")]
        [Range(0, 60, ErrorMessage = "時間必須介於0~60分鐘")]
        [Required(ErrorMessage = "必須輸入時間")]
        public int ID_DISABLE_IN_CONT_TIMES { get; set; }

        /// <summary>
        /// 同一天密碼非連續錯誤次數，超過次數，則設定無效（帳戶鎖定）
        /// </summary>
        [DisplayName("同一天密碼非連續錯誤次數限制")]
        [RegularExpression(@"^[\d]*$", ErrorMessage = "請輸入正整數")]
        [Range(0, 20, ErrorMessage = "次數必須介於0~20之間")]
        [Required(ErrorMessage = "必須輸入次數")]
        public int ID_DISABLE_IN_NONCONT { get; set; }

        /// <summary>
        /// 同一天密碼非連續錯誤超過限制之帳號鎖定時間限制
        /// </summary>
        [DisplayName("同一天密碼非連續錯誤超過限制之帳號鎖定時間限制")]
        [RegularExpression(@"^[\d]*$", ErrorMessage = "請輸入正整數")]
        [Range(0, 60, ErrorMessage = "時間必須介於1~60分鐘")]
        [Required(ErrorMessage = "必須輸入時間")]
        public int ID_DISABLE_IN_NONCONT_TIMES { get; set; }
    }
}
