using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 廠商資訊
    /// </summary>
    public class ProjectTenderModel : DbEditor
    {
        /// <summary>
        /// 流水編號
        /// </summary>
        public int SEQ { get; set; }

        /// <summary>
        /// 列管編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 廠商類別
        /// </summary>
        public string TENDER_KIND { get; set; }

        /// <summary>
        /// 廠商類別中文
        /// </summary>
        public string TENDER_KIND_NAME { get; set; }

        /// <summary>
        /// 廠商名稱
        /// </summary>
        public string TENDER_NAME { get; set; }

        /// <summary>
        /// 統編
        /// </summary>
        public string REG_NO { get; set; }

        /// <summary>
        /// 廠商地址
        /// </summary>
        public string TENDER_ADDR { get; set; }

        /// <summary>
        /// 連絡人
        /// </summary>
        public string CONTACT_NAME { get; set; }

        /// <summary>
        /// 連絡人電話
        /// </summary>
        public string CONTACT_PHONE { get; set; }

        /// <summary>
        /// 連絡人信箱
        /// </summary>
        public string CONTACT_EMAIL { get; set; }
    }
}
