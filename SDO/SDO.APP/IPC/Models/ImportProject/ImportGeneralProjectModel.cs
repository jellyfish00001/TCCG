using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace SDO.Models
{
    public class ImportGeneralProjectModel:DbEditor
    {
        /// <summary>
        /// 年度
        /// </summary>
        public string PlanYear { get; set; }
        /// <summary>
        /// 匯入檔案
        /// </summary>
        public IFormFile ImportFile { get; set; }
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }
        /// <summary>
        /// 辦理內容
        /// </summary>
        public string ALL_JOB { get; set; }
        /// <summary>
        /// 中央補助經費(千元)
        /// </summary>
        public decimal BUDGET_CENTRAL { get; set; }
        /// <summary>
        /// 縣市自籌經費(千元)
        /// </summary>
        public decimal BUDGET_LOCAL { get; set; }
        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PROJECT_NAME { get; set; }
        /// <summary>
        /// 地方自籌款來源說明
        /// </summary>
        public string BUDGET_LOCAL_SOURCE { get; set; }
        /// <summary>
        /// 主管機關代碼
        /// </summary>
        public string MASTER_ORGAN_C { get; set; }
        /// <summary>
        /// 執行機關代碼
        /// </summary>
        public string EXEC_ORGAN_C { get; set; }
    }
}
