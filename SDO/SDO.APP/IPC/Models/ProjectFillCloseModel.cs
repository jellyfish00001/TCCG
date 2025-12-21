using SDO.Base.Utils.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫結案資料Model
    /// </summary>
    public class ProjectFillCloseModel : DbEditor
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 計畫狀態
        /// </summary>
        public string PROJECT_STATUS { get; set; }

        /// <summary>
        /// 審查結果
        /// </summary>
        public string REVIEW_RESULT { get; set; }

        /// <summary>
        /// 管考意見
        /// </summary>
        public string REVIEW_COMMENTS { get; set; }

        /// <summary>
        /// 管考意見
        /// </summary>
        public string MEMO_CLOSE { get; set; }

        /// <summary>
        /// 經費支出時間
        /// </summary>
        public DateTime? DATA_DATE
        {
            get
            {
                if (string.IsNullOrEmpty(DATA_DATE_SHOW))
                {
                    return null;
                }
                else
                {
                    int year = Convert.ToInt32(DATA_DATE_SHOW.Substring(0, 3)) + 1911;
                    int month = Convert.ToInt32(DATA_DATE_SHOW.Substring(4, 2));
                    return new DateTime(year, month, 1).Date;
                }
            }
        }

        /// <summary>
        /// 經費支出年月 (畫面顯示)
        /// </summary>
        public string DATA_DATE_SHOW { get; set; }

        public string DATA_DATE_FORMAT
        {
            get
            {

                if (DATA_DATE.HasValue)
                {
                    string year = (DATA_DATE.Value.Year - 1911).ToString();
                    string month = DATA_DATE.Value.Month.ToString("00");
                    return $"{year}_{month}";
                }
                return "";
            }
        }

        /// <summary>
        /// 異動狀態
        /// </summary>
        public string LOG_STATUS { get; set; }

        /// <summary>
        /// 計畫總經費
        /// </summary>
        public long TOTAL_BUDGET { get; set; }

        /// <summary>
        /// 計畫實際經費支用識別碼
        /// </summary>
        public int PAYMENT_ID { get; set; }

        /// <summary>
        /// 累計實際完成金額(元)
        /// </summary>
        [Required(ErrorMessage = "「累計實際完成金額」")]
        public decimal? TOTAL_ACTUAL_COMP { get; set; }

        /// <summary>
        /// 累計實際支用(元)
        /// </summary>
        [Required(ErrorMessage = "「累計實際支用」")]
        public decimal? ACTUAL_PAY { get; set; }

        /// <summary>
        /// 應付未付款(元)
        /// </summary>
        [Required(ErrorMessage = "「應付未付款」")]
        public decimal? UNPAY { get; set; }

        /// <summary>
        /// 節餘數(元)
        /// </summary>
        [Required(ErrorMessage = "「節餘數」")]
        public decimal? BALANCE { get; set; }

        /// <summary>
        /// 經費填報日期
        /// </summary>
        public DateTime? MDF_DATE { get; set; }

        /// <summary>
        /// 佐證資料檔案
        /// </summary>
        public List<ProjectAttachmentModel> ProjAttachments { get; set; }

        /// <summary>
        /// 是否為結案審核
        /// </summary>
        public bool isAudit { get; set; }

        /// <summary>
        /// 是否為確認送出
        /// </summary>
        public bool isSubmit { get; set; }

        /// <summary>
        /// 異動檔案
        /// </summary>
        public List<UploadTempFileModel> EditFiles { get; set; }

        /// <summary>
        /// 結案日
        /// </summary>
        public DateTime? FINISH_DATE { get; set; }

        /// <summary>
        /// 可否存檔
        /// </summary>
        public bool CanSave { get; set; }

        /// <summary>
        /// 計畫歷程清單
        /// </summary>
        public List<ProjectLogListModel> ProjLogs { get; set; }

    }
}
