using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 計畫基本驗證Model
    /// </summary>
    public class ProjectBasicCheckModel: ProjectBasicModel
    {
        /// <summary>
        /// 計畫名稱
        /// </summary>
        [Required(ErrorMessage = "「計畫名稱」")]
        public new string PROJECT_NAME { get; set; }
        /// <summary>
        /// 計畫年度
        /// </summary>
        [Required(ErrorMessage = "「計畫年度」")]
        public new string PROJECT_YEAR { get; set; }

        /// <summary>
        /// 主管機關
        /// </summary>
        [Required(ErrorMessage = "「主管機關」")]
        public new string MASTER_ORGAN_C { get; set; }

        /// <summary>
        /// 主管機關人員/主管機關承辦人
        /// </summary>
        [Required(ErrorMessage = "「主管機關人員」")]
        public new string MASTER_UNDERTAKER_C { get; set; }

        /// <summary>
        /// 執行機關
        /// </summary>
        [Required(ErrorMessage = "「執行機關」")]
        public new string EXEC_ORGAN_C { get; set; }

        /// <summary>
        /// 執行機關人員/執行機關承辦人
        /// </summary>
        [Required(ErrorMessage = "「執行機關人員」")]
        public new string EXEC_UNDERTAKER_C { get; set; }

        /// <summary>
        /// 相關審查
        /// </summary>
        [Required(ErrorMessage = "「相關審查」")]
        public new string REVIEWITEM { get; set; }

        /// <summary>
        /// 辦理地點
        /// </summary>
        [Required(ErrorMessage = "「辦理地點」")]
        public new string TOWN_C { get; set; }

        /// <summary>
        /// 位置說明
        /// </summary>
        [Required(ErrorMessage = "「位置說明」")]
        public new string PROJECT_LOCATION { get; set; }

        /// <summary>
        /// 單點地圖定位-坐標X
        /// </summary>
        [Required(ErrorMessage = "「單點地圖定位-坐標X」")]
        public new string X_COORD { get; set; }

        /// <summary>
        /// 單點地圖定位-坐標Y
        /// </summary>
        [Required(ErrorMessage = "「單點地圖定位-坐標Y」")]
        public new string Y_COORD { get; set; }

        /// <summary>
        /// 計畫內容
        /// </summary>
        [Required(ErrorMessage = "「計畫內容」")]
        public new string ALL_JOB { get; set; }

        /// <summary>
        /// 計畫效益
        /// </summary>
        [Required(ErrorMessage = "「計畫效益」")]
        public new string PROJECT_BENEFIT { get; set; }
    }
}
