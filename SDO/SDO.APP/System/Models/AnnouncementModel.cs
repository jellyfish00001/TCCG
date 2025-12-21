using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SDO.Models
{
    public class AnnouncementModel : DbEditor
    {
        /// <summary>
        /// 公告代碼
        /// </summary>
        public string SID { get; set; }

        /// <summary>
        /// 公告主旨
        /// </summary>
        //[Required]
        //[Display(Name = "Announcement_TITLE", ResourceType = typeof(i18N.Label))]
        public string TITLE { get; set; }

        /// <summary>
        /// 公告內容
        /// </summary>
        [Display(Name = "Announcement_COMMENT", ResourceType = typeof(i18N.Label))]
        public string COMMENT { get; set; }

        /// <summary>
        /// 公告起始日
        /// </summary>
        //[Required]
        //[Display(Name = "Announcement_DATE", ResourceType = typeof(i18N.Label))]
        public DateTime EFFECTIVE_DATE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string EFFECTIVE_TWDATE
        {
            get
            {
                return string.Format("{0}/{1}", this.EFFECTIVE_DATE.Year - 1911, this.EFFECTIVE_DATE.ToString("MM/dd"));
            }
            set { }
        }

        /// <summary>
        /// 公告到期日
        /// </summary>
        //[Required]
        //[Display(Name = "Announcement_DATE", ResourceType = typeof(i18N.Label))]
        public DateTime EXPIRE_DATE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string EXPIRE_TWDATE
        {
            get
            {
                return string.Format("{0}/{1}", this.EXPIRE_DATE.Year - 1911, this.EXPIRE_DATE.ToString("MM/dd"));
            }
            set { }
        }

        /// <summary>
        /// 附件名稱
        /// </summary>
        //[Display(Name = "Announcement_ATTACHMENT", ResourceType = typeof(i18N.Label))]
        public string ATTACH_NAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string OFF_DOC { get; set; }

        /// <summary>
        /// 公告類別
        /// </summary>
        public string ANN_TYPE { get; set; }

        /// <summary>
        /// 公告類別名稱
        /// </summary>
        public string ANN_TYPE_NAME { get; set; }

        /// <summary>
        /// 是否為外部連結 Y:是 N:否
        /// </summary>
        public string IS_URL_LINK { get; set; }

        /// <summary>
        /// 連結網址
        /// </summary>
        public string URL_LINK { get; set; }

        /// <summary>
        /// 登入者ORG_ID
        /// </summary>
        public string ORG_ID { get; set; }
        /// <summary>
        /// 登入者USER_ID
        /// </summary>
        public string USER_ID { get; set; }

        public DateTime MDF_DATE { get; set; }

    }
}