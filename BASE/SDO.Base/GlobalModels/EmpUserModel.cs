using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Security.Permissions;

namespace SDO.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class EmpUserModel : DbEditor
    {
        public string USER_ID { get; set; }
        public string USER_NAME { get; set; }
        public string USER_EMAIL { get; set; }
        public string USER_PD { get; set; }
        public string[] ROLES { get; set; }

        /// <summary>
        /// 組織
        /// </summary>
        public string ORG_Id { get; set; }
    }
}
