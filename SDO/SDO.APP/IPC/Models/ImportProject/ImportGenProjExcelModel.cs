using System.Collections.Generic;

namespace SDO.Models
{
    public class ImportGenProjExcelModel
    {
        public int Index { get; set; }
        /// <summary>
        /// 縣市別名稱
        /// </summary>
        public string CITY_NAME { get; set; }
        /// <summary>
        /// 縣市別代碼
        /// </summary>
        public string CITY_GOV_ID { get; set; }
        /// <summary>
        /// 辦理內容
        /// </summary>
        public string ALL_JOB { get; set; }
        /// <summary>
        /// 中央補助經費(千元)
        /// </summary>
        public string BUDGET_CENTRAL { get; set; }
        /// <summary>
        /// 縣市自籌經費(千元)
        /// </summary>
        public string BUDGET_LOCAL { get; set; }
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

        public List<string> ErrMsgs { get; set; }



    }
}
