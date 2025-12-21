using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    [Serializable]
    public class SCUserModel : SCUser, ICloneable, ISCUserData
    {
        public string USER_ID { get { return UsrID; } set { UsrID = value; } }


        public string USER_NAME { get { return UsrName; } set { UsrName = value; } }

        public string USER_PD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CONFIRM_USER_PD { get; set; }

        public string USER_IP
        {
            get { return UsrIP; }
            set { UsrIP = value; }
        }

        public string USER_EMAIL { get { return UsrEmail; } set { UsrEmail = value; } }

        public string USER_TEL
        {
            get { return UsrCustiom1; }
            set { UsrCustiom1 = value; }
        }


        /// <summary>
        /// 限制使用者來源IP (用DB.PSWD_Q放資料)
        /// </summary>
        public string LIMIT_LOCATION { get; set; }


        public string USER_MACHINE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AGENT_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool DEL_FLG
        {
            get { return IDEnable == "Y"; }
            set { IDEnable = value ? "Y" : "N"; }
        }

        public string IS_ENABLED
        {
            get { return IDEnable; }
            set { IDEnable = value; }
        }
        /// <summary>
        /// 組織
        /// </summary>
        public string ORG_ID { get { return OrgOuID; } set { OrgOuID = value; } }

        /// <summary>
        /// 機關名稱
        /// </summary>
        public string ORG_NAME { get { return OrgOuName; } set { OrgOuName = value; } }

        /// <summary>
        /// 角色
        /// </summary>
        public string ROLE_ID { get; set; }

        public SCUser ToSCUser()
        {
            return (SCUser)base.Clone();
        }
    }
    public class SCUser : ICloneable
    {
        public object Clone()
        {
            return MemberwiseClone();
        }
        /// <summary>
        /// 使用者代號
        /// </summary>
        /// <value>使用者代號</value>
        public string UsrID { get; set; }

        /// <summary>
        /// 使用者公司代號
        /// </summary>
        /// <value>使用者公司代號</value>
        public string UsrCompID { get; set; }

        /// <summary>
        /// 使用者名稱
        /// </summary>
        /// <value>使用者名稱</value>
        public string UsrName { get; set; }

        /// <summary>
        /// 使用者職稱
        /// </summary>
        /// <value>使用者職稱</value>
        public string UsrTitle { get; set; }

        /// <summary>
        /// 使用者IP
        /// </summary>
        /// <value>使用者職稱</value>
        public string UsrIP { get; set; }

        /// <summary>
        /// 使用者類別
        /// </summary>
        /// <value>使用者類別</value>
        public string UsrType { get; set; }

        /// <summary>
        /// 使用者說明
        /// </summary>
        /// <value>使用者說明</value>
        public string UsrDesc { get; set; }

        /// <summary>
        /// 使用者職級
        /// </summary>
        /// <value>使用者職級</value>
        public string UsrGrade { get; set; }

        /// <summary>
        /// 使用者代號之有效狀態
        /// </summary>
        /// <value>使用者代號之有效狀態</value>
        public string IDEnable { get; set; }

        /// <summary>
        /// 須變更密碼
        /// </summary>
        /// <value>須變更密碼</value>
        public string NeedChanpswd { get; set; }

        /// <summary>
        /// 電腦設備名稱
        /// </summary>
        /// <value>電腦設備名稱</value>
        public string CurLocation { get; set; }

        /// <summary>
        /// 生效日期時間
        /// </summary>
        /// <value>生效日期時間</value>
        public DateTime? SDate { get; set; }

        /// <summary>
        /// 無效日期時間
        /// </summary>
        /// <value>無效日期時間</value>
        public DateTime? EDate { get; set; }

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
        /// 密碼變更次數 (計數用)
        /// </summary>
        /// <value>密碼變更次數 (計數用)</value>
        public int? Counter { get; set; }

        /// <summary>
        /// 身份證
        /// </summary>
        /// <value>身份證</value>
        public string IDNO { get; set; }

        /// <summary>
        /// 員工編號
        /// </summary>
        /// <value>員工編號</value>
        public string EMPID { get; set; }

        /// <summary>
        /// 使用者別名
        /// </summary>
        /// <value>使用者別名</value>
        public string UsrNickname { get; set; }

        /// <summary>
        /// 使用者電子郵件
        /// </summary>
        /// <value>使用者電子郵件</value>
        public string UsrEmail { get; set; }


        /// <summary>
        /// 額外欄位1
        /// </summary>
        /// <value>額外欄位1</value>
        public string UsrCustiom1 { get; set; }

        /// <summary>
        /// 額外欄位2
        /// </summary>
        /// <value>額外欄位2</value>
        public string UsrCustiom2 { get; set; }

        /// <summary>
        /// 額外欄位3
        /// </summary>
        /// <value>額外欄位3</value>
        public string UsrCustiom3 { get; set; }

        /// <summary>
        /// 組織單位代號(主辦單位)
        /// </summary>
        /// <value>組織單位代號(主辦單位)</value>
        public string OuID { get; set; }

        /// <summary>
        /// 組織單位名稱(主辦單位)
        /// </summary>
        /// <value>組織單位名稱(主辦單位)</value>
        public string OuName { get; set; }

        /// <summary>
        /// 組織單位類型 1-主管機關 2-主辦機關 3-主辦單位 4-其他單位 
        /// </summary>
        public string OuKind { get; set; }

        /// <summary>
        /// 主辦機關代號
        /// </summary>
        /// <value>主辦機關代號</value>
        public string UnitOuID { get; set; }

        /// <summary>
        /// 主辦機關名稱
        /// </summary>
        /// <value>主辦機關名稱</value>
        public string UnitOuName { get; set; }

        /// <summary>
        /// 主管機關代號
        /// </summary>
        /// <value>主管機關代號</value>
        public string OrgOuID { get; set; }

        /// <summary>
        /// 主管機關名稱
        /// </summary>
        /// <value>主管機關名稱</value>
        public string OrgOuName { get; set; }



        /// <summary>
        /// 取得登入使用者之指定子系統角色清單
        /// </summary>
        /// <returns></returns>
        public List<string> Roles { get; set; }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetUSER_DISPLAY_OWNER()
        {
            return string.Format("{0} ({1})-{2}", this.UnitOuName, this.OuName, this.UsrName);
        }
    }
}
