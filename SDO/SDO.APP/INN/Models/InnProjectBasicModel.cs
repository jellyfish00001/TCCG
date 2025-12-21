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
    /// 提案基本資料
    /// </summary>
    public class InnProjectBasicModel : DbEditor
    {
        /// <summary>
        /// 年度
        /// </summary>
        public string INN_YEAR { get; set; }

        public string OU_ID { get; set; }

        /// <summary>
        /// 提案序號
        /// </summary>
        public string INN_PLAN_NO { get; set; }

        /// <summary>
        /// 提案人數
        /// </summary>
        public string SPONSOR_TYPE { get; set; }

        /// <summary>
        /// 組別
        /// </summary>
        public string GROUP { get; set; }

        /// <summary>
        /// 提案名稱
        /// </summary>
        public string INN_PLAN_NAME { get; set; }

        /// <summary>
        /// 是否有實施提案所定不受理範圍之各項情形
        /// </summary>
        public string REJECT_YN { get; set; }

        /// <summary>
        /// 問題描述
        /// </summary>
        public string INN_DESCRIPTION { get; set; }

        /// <summary>
        /// 提案構想解決方式
        /// </summary>
        public string IDEA_CONTENT { get; set; }

        /// <summary>
        /// 預期效益
        /// </summary>
        public string EXPECT_BENEFIT { get; set; }

        /// <summary>
        /// 是否為本府尚未推行過之創意
        /// </summary>
        public string SPREAD_IDEA_YN { get; set; }

        /// <summary>
        /// 執行機關
        /// </summary>
        public string ORIGINATE_YN { get; set; }

        /// <summary>
        /// 主要提案人員-機關
        /// </summary>
        public string SPONSOR_ORG { get; set; }

        /// <summary>
        /// 主要提案人員-單位
        /// </summary>
        public string SPONSOR_UNIT { get; set; }

        /// <summary>
        /// 主要提案人員-職稱
        /// </summary>
        public string SPONSOR_TITLE { get; set; }

        /// <summary>
        /// 主要提案人員-姓名
        /// </summary>
        public string SPONSOR_NAME { get; set; }

        /// <summary>
        /// 主要提案人員-性別
        /// </summary>
        public string SPONSOR_SEX { get; set; }

        /// <summary>
        /// 聯絡人
        /// </summary>
        public string CONTACT_NAME { get; set; }

        /// <summary>
        /// 聯絡人電話
        /// </summary>
        public string CONTACT_TEL { get; set; }

        /// <summary>
        /// 聯絡人EMAIL
        /// </summary>
        public string CONTACT_EMAIL { get; set; }

        public string CONTACT_ORG { get; set; }

        public string CONTACT_TITLE { get; set; }

        /// <summary>
        /// 是否涉及
        /// </summary>
        public string IS_PLURAL { get; set; }

        public string PROPOSAL_TYPE { get; set; }


    }
}
