using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 表1 各年度提案資料清冊
    /// </summary>
    public class PlanListModel
    {
        /// <summary>
        /// 提案編號
        /// </summary>
        public string INN_PLAN_NO { get; set; }
        /// <summary>
        /// 提案名稱
        /// </summary>
        public string INN_PLAN_NAME { get; set; }
        /// <summary>
        /// 提案年度
        /// </summary>
        public string INN_YEAR { get; set; }
        /// <summary>
        /// 提案人數
        /// </summary>
        public string SPONSOR_TYPE { get; set; }
        /// <summary>
        /// 參加組別
        /// </summary>
        public string GROUP { get; set; }
        /// <summary>
        /// 提案主題
        /// </summary>
        public string PROPOSALTYPE_MAIN { get; set; }
        /// <summary>
        /// 次要提案分類
        /// </summary>
        public string PROPOSALTYPE_SUB { get; set; }
        /// <summary>
        /// 提案機關
        /// </summary>
        public string OU_NAME { get; set; }
        /// <summary>
        /// 主要提案人
        /// </summary>
        public string SPONSOR_NAME { get; set; }
        
        /// <summary>
        /// 主要提案人性別
        /// </summary>
        public string SPONSOR_SEX { get; set; }

        /// <summary>
        /// 聯絡人單位
        /// </summary>
        public string CONTACT_ORG { get; set; }

        /// <summary>
        /// 聯絡人職稱
        /// </summary>
        public string CONTACT_TITLE { get; set; }

        /// <summary>
        /// 聯絡人姓名
        /// </summary>
        public string CONTACT_NAME {get; set; }

        /// <summary>
        /// 聯絡人電話
        /// </summary>
        public string CONTACT_TEL { get; set; }

        /// <summary>
        /// 聯絡人email
        /// </summary>
        public string CONTACT_EMAIL { get; set; }
    }
}
