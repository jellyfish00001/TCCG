using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class BasicDataModel
    {
        public string USER_ID { get; set; }
        public string ORG_ID { get; set; }
        public string EMAIL { get; set; }
        public IList<DimRoleModel> ROLES { get; set; }
        public string USR_TYPE { get; set; }
        public IList<DimRoleModel> AllROLES { get; set; }
    }
}
