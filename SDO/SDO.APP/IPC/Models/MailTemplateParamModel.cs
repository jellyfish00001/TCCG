
using System.ComponentModel;

namespace SDO.Models
{
    /// <summary>
    /// 郵件替換參數資料Model
    /// </summary>
    public class MailTemplateParamModel
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        [DisplayName("計畫編號")]
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        [DisplayName("計畫名稱")]
        public string PROJECT_NAME { get; set; }

        /// <summary>
        /// 執行機關
        /// </summary>
        [DisplayName("執行機關")]
        public string EXEC_ORGAN_NAME { get; set; }

        /// <summary>
        /// 執行機關承辦人
        /// </summary>
        [DisplayName("執行機關承辦人")]
        public string EXEC_UNDERTAKER_NAME { get; set; }

        /// <summary>
        /// 執行機關承辦人電話
        /// </summary>
        [DisplayName("執行機關承辦人電話")]
        public string EXEC_UNDERTAKER_TEL { get; set; }

        /// <summary>
        /// 計畫實際承辦人
        /// </summary>
        [DisplayName("計畫實際承辦人")]
        public string REAL_CONTACT { get; set; }

        /// <summary>
        /// 計畫實際承辦人電話
        /// </summary>
        [DisplayName("計畫實際承辦人電話")]
        public string REAL_TEL { get; set; }

        /// <summary>
        /// 登入者姓名
        /// </summary>
        [DisplayName("姓名")]
        public string RDEC_NAME { get; set; }

        /// <summary>
        /// 登入者電話
        /// </summary>
        [DisplayName("管考電話")]
        public string RDEC_TEL { get; set; }

        /// <summary>
        /// 填報月份
        /// </summary>
        [DisplayName("填報月份")]
        public string FILL_MONTH { get; set; }

        /// <summary>
        /// 管考意見
        /// </summary>
        [DisplayName("管考意見")]
        public string AUDIT_OPINION { get; set; }

        /// <summary>
        /// 管考備註
        /// </summary>
        [DisplayName("管考備註")]
        public string AUDIT_MEMO { get; set; }

        /// <summary>
        /// 立案結案管考意見
        /// </summary>
        [DisplayName("立案結案管考意見")]
        public string MEMO_EVALUATION { get; set; }

        /// <summary>
        /// 調整撤銷管考意見
        /// </summary>
        [DisplayName("調整撤銷管考意見")]
        public string REVIEW_COMMENTS { get; set; }
    }
}
