using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.IPC.Models.ProjectAdjust
{
    /// <summary>
    /// 主辦申請調整撤銷原因Model
    /// </summary>
    public class AdjustReasonModel : DbEditor
    {
        /// <summary>
        /// 計畫調整檔流水號
        /// </summary>
        public int PROJ_ADJ_ID { set; get; }
        /// <summary>
        /// 列管編號
        /// </summary>
        public string PROJECT_NO { set; get; }
        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PROJECT_NAME { set; get; }
        /// <summary>
        /// 調整申請項目 (SET_PARAM.SET_ITEM = 'AW_KIND')
        /// </summary>
        public string AW_KIND { set; get; }
        /// <summary>
        /// 核准日期
        /// </summary>
        public DateTime? APPRV_DATE { set; get; }
        /// <summary>
        /// 計畫調整原因說明
        /// </summary>
        public string ADJUST_REASON { set; get; }
        /// <summary>
        /// 其他調整原因
        /// </summary>
        public string OTHER_REASON { set; get; }
        /// <summary>
        /// 調整撤銷狀態
        /// </summary>
        public string PROJECT_AW_STATUS { set; get; }
        /// <summary>
        /// 總計畫經費 – 總預算經費
        /// </summary>
        public decimal BUDGET { set; get; }
        /// <summary>
        /// 總計畫經費 – 發包金額
        /// </summary>
        public decimal PROCUREMENT_AMT { set; get; }
        /// <summary>
        /// 總計畫經費 – 決標金額
        /// </summary>
        public decimal TENDER_AWARDING_AMT { set; get; }
        /// <summary>
        /// 承包廠商 – 專案管理
        /// </summary>
        public string TENDER_PROJ { set; get; }
        /// <summary>
        /// 承包廠商 – 設計單位
        /// </summary>
        public string TENDER_DESIGN { set; get; }
        /// <summary>
        /// 承包廠商 – 監造單位
        /// </summary>
        public string TENDER_SUPV { set; get; }
        /// <summary>
        /// 承包廠商 – 施工單位
        /// </summary>
        public string TENDER_CONST { set; get; }
        /// <summary>
        /// 有無獲得中央補助款
        /// </summary>
        public int? IS_BUDGET_CENTRAL { set; get; }
        /// <summary>
        /// 無核定函原因
        /// </summary>
        public string NO_OD_REASON { set; get; }
        /// <summary>
        /// 有無影響補助經費請領
        /// </summary>
        public int? IS_EFFECT_BUDGET { set; get; }
        /// <summary>
        /// 影響經費請領說明
        /// </summary>
        public string EFFECT_BUDGET_MEMO { set; get; }
        /// <summary>
        /// 目前執行情形
        /// </summary>
        public string CUR_EXECUTION { set; get; }
        /// <summary>
        /// 調整原因/撤銷原因 (用於存入PROJECT_MAPPING_DATA)
        /// </summary>
        public List<ProjectMappingDataModel> Reasons { set; get; }
        /// <summary>
        /// 檔案2: 上傳核定函(for 期程調整事由) / 准簽(for 期程送審)
        /// </summary>
        public ProjectAttachmentModel Files2 { set; get; }
        /// <summary>
        /// 檔案: 佐證資料 / 已核章申請表(for 期程送審)
        /// </summary>
        public ProjectAttachmentModel Files { set; get; }
    }
}
