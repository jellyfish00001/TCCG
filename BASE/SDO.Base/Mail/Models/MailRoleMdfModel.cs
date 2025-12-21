using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class MailRoleMdfModel : MailRoleModel, IDbEditor
    {
        public string[] USERS { get; set; }
        public string CRT_USER { get; set; }
        public string MDF_USER { get; set; }
    }
}
