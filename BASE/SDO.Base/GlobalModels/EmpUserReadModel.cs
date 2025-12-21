using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class EmpUserReadModel
    {
        /// <summary>
        /// 帳號
        /// </summary>
        public string USER_ID { get; set; }
        /// <summary>
        /// 名稱
        /// </summary>
        public string USER_NAME { get; set; }
        /// <summary>
        /// 組織
        /// </summary>
        public string ORG_ID { get; set; }
        /// <summary>
        /// E-mail
        /// </summary>
        public string USER_EMAIL { get; set; }
        public string IS_LAST_SUCCLOGIN { get; set; }
        /// <summary>
        /// 天數
        /// </summary>
        public int? DAY_COUNT { get; set; }
        /// <summary>
        /// 角色
        /// </summary>
        public string ROLE_ID { get; set; }
        /// <summary>
        /// 公司統編
        /// </summary>
        public string COMP_ID { get; set; }
        /// <summary>
        /// 公司名稱
        /// </summary>
        public string COMP_NAME { get; set; }
        public int DEL_FLG { get; set; }
    }
}
