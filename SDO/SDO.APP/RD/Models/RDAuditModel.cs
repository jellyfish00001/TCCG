using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 審查資料檔 Model
    /// </summary>
    public class RDAuditModel : DbEditor
    {
        /// <summary>
        /// 自動流水號
        /// </summary>
        public int SEQ { get; set; }

        /// <summary>
        /// 審查紀錄編號
        /// </summary>
        public string AUDIT_ID { get; set; }

        /// <summary>
        /// 列管編號(計畫編號 PLAN_NO)
        /// </summary>
        public string MAIN_NO { get; set; }

        /// <summary>
        /// 副編號，對應是明細的seq使用
        /// </summary>
        public int SUB_NO { get; set; }

        /// <summary>
        /// 審查狀態
        /// </summary>
        public string STATUS { get; set; }

        /// <summary>
        /// 審查類別 - 參數代碼檔
        /// </summary>
        /// <remarks>
        /// 基本資料A1、執行情形填報A2、結案成果填報A3、續列管一年內參採情形A4、展延申請A5
        /// </remarks>
        public string PLAN_REVIEW_TYPE { get; set; }

        /// <summary>
        /// 審查類別 - 轉參數名稱
        /// </summary>
        /// <remarks>
        /// 基本資料A1、執行情形填報A2、結案成果填報A3、續列管一年內參採情形A4、展延申請A5
        /// </remarks>
        public string PLAN_REVIEW { get; set; }

        /// <summary>
        /// 審查結果 - 轉參數名稱
        /// </summary>
        public string REVIEW_RESULT_STATUS { get; set; }

        /// <summary>
        /// 審查結果 - 參數代碼檔
        /// </summary>
        /// <remarks>
        /// Y：核可通過(審核通過)、R：退回補正、N：審核未通過
        /// </remarks>
        public string REVIEW_RESULT { get; set; }

        /// <summary>
        /// 審查意見
        /// </summary>
        public string REVIEW_COMMENTS { get; set; }

        /// <summary>
        /// 是否確認送出（送出審核），1:確認送出 0:存檔
        /// </summary>
        public int IS_SEND { get; set; }

        /// <summary>
        /// 送審申請年度
        /// </summary>
        /// <remarks>產生審查紀錄編號時需要使用到當下民國年度</remarks>
        public string AUDIT_YEAR { get; set; }

        /// <summary>
        /// 送審申請月份
        /// </summary>
        /// <remarks>產生審查紀錄編號時需要使用到當下月份</remarks>
        public string AUDIT_MONTH { get; set; }

        /// <summary>
        /// 委託研究的基本資料 Model
        /// </summary>
        public ResearchBasicModel RESEARCH_BASIC_MODEL { get; set; }

        /// <summary>
        /// 執行情形明細 Model
        /// </summary>
        public ResPolicyIndexModel RES_POLICY_INDEX_MODEL { get; set; }

        /// <summary>
        /// 參採情形/結案成果填報/續列管一年參採情形 Model
        /// </summary>
        public ResSituationModel RES_SITUATION_MODEL { get; set; }

        /// <summary>
        /// 展延申請 Model
        /// </summary>
        public ProjectExtensionModel PROJECT_EXTENSION_MODEL { get; set; }
    }
}
