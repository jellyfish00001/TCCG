using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 表 13 重大建設系統介接公共工程雲雲端服務網資料一覽表
    /// </summary>
    public class ProjectSyncLogOverviewModel : ProjectSyncLogModel
    {
        /// <summary>
        /// 界接狀況 名稱
        /// </summary>
        public string SET_VALUE { get; set; }

        /// <summary>
        /// 標案編號
        /// </summary>
        public string PCC_PROJECT_NO { get; set; }

        /// <summary>
        /// 標案識別碼
        /// </summary>
        public string PCC_PROJECT_UID { get; set; }

        /// <summary>
        /// 本月執行情形
        /// </summary>
        public string EXECUTE_CONDITION { get; set; }

        /// <summary>
        /// 需協辦事項
        /// </summary>
        public string ASSISTANT_ITEM { get; set; }

        /// <summary>
        /// 預定開工日期
        /// </summary>
        public string START_ESTIMATED_ENDDATE { get; set; }

        /// <summary>
        /// 實際開工日期
        /// </summary>
        public string START_ACTUAL_ENDDATE { get; set; }

        /// <summary>
        /// 預定竣工日期
        /// </summary>
        public string COMPLETION_ESTIMATED_ENDDATE { get; set; }

        /// <summary>
        /// 預定驗收日期
        /// </summary>
        public string ACCEPT_ESTIMATED_ENDDATE { get; set; }

        /// <summary>
        /// 實際驗收日期
        /// </summary>
        public string ACCEPT_ACTUAL_ENDDATE { get; set; }

        /// <summary>
        /// 標案系統預定開工日期
        /// </summary>
        public string START_PCC_ESTIMATED_ENDDATE { get; set; }

        /// <summary>
        /// 標案系統實際開工日期
        /// </summary>
        public string START_PCC_ACTUAL_ENDDATE { get; set; }

        /// <summary>
        /// 標案系統預定竣工日期
        /// </summary>
        public string COMPLETION_PCC_ESTIMATED_ENDDATE { get; set; }

        /// <summary>
        /// 標案系統實際竣工日期
        /// </summary>
        public string COMPLETION_PCC_ACTUAL_ENDDATE { get; set; }

        /// <summary>
        /// 標案預定施工進度
        /// </summary>
        public decimal? TEN_RES_PRG { get; set; }

        /// <summary>
        /// 標案實際施工進度
        /// </summary>
        public decimal? TEN_ACT_PRG { get; set; }

        /// <summary>
        /// 標案工程進度 差異
        /// </summary>
        public decimal? TEN_DIFF_PRG
        {
            get
            {
                if (TEN_RES_PRG.HasValue && TEN_ACT_PRG.HasValue)
                {
                    return TEN_RES_PRG.Value - TEN_ACT_PRG.Value;
                }
                return null;
            }
        }

        /// <summary>
        /// 落後類型
        /// </summary>
        public string DELAY_KIND { get; set; }

        /// <summary>
        /// 落後類別
        /// </summary>
        public string DELAY_CLASS_C { get; set; }

        /// <summary>
        /// 落後項目
        /// </summary>
        public string DELAY_SUBCLASS_C { get; set; }

        /// <summary>
        /// 責任歸屬
        /// </summary>
        public string DELAY_RESPON { get; set; }

        /// <summary>
        /// 落後原因
        /// </summary>
        public string DELAY_CAUSAL { get; set; }

        /// <summary>
        /// 解決對策
        /// </summary>
        public string SOLUTION { get; set; }

        /// <summary>
        /// 須協調事項
        /// </summary>
        public string COORDINATION { get; set; }

        /// <summary>
        /// 改進完成期限
        /// </summary>
        public string DEADLINES { get; set; }

    }
}
