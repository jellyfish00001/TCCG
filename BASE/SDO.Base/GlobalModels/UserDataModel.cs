using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class UserDataModel : ICloneable, IUserData
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Object Clone()
        {
            return this.MemberwiseClone();
        }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [Display(Name = "帳號")]
        public string USER_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [Display(Name = "姓名")]
        public string USER_NAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "電子郵件")]
        [EmailAddress]
        public string USER_EMAIL { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "密碼")]
        public string USER_PD { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "確認密碼")]
        [System.ComponentModel.DataAnnotations.Compare("USER_PD")]
        public string CONFIRM_USER_PD { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string USER_IP { get; set; }

        public string USER_MACHINE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AGENT_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool DEL_FLG { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime LAST_CHANPSWD { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime LAST_SUCCLOGIN { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime LAST_FAILLOGIN { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int CNT_FAILLOGIN { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int CON_FAULT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int NONCON_FAULT { get; set; }

        /// <summary>
        /// 組織 ( 廠商那邊為統編 )
        /// </summary>
        public string ORG_ID { get; set; }
        /// <summary>
        /// 電話
        /// </summary>
        public string USER_TEL { get; set; }

        /// <summary>
        /// 職稱
        /// </summary>
        public string UsrTitle { get; set; }

        /// <summary>
        /// 機關名稱
        /// </summary>
        public string ORG_NAME { get; set; }

        /// <summary>
        /// 單位ID
        /// </summary>
        public string OuID { get; set; }

        public string IS_ENABLED { get; set; }

        /// <summary>
        /// 角色
        /// </summary>
        public string ROLE_ID { get; set; }

        /// <summary>
        /// 取得登入使用者之指定子系統角色清單
        /// </summary>
        public List<string> Roles { get; set; }
        public string UsrType { get; set; }
    }
}
