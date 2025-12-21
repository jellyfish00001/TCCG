using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public interface ISCRoleDac : IUserData
    {
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
    }
}

