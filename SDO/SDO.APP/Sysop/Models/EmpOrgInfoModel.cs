using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class EmpOrgInfoModel
    {
        public string ORG_ID { get; set; }
        public string ORG_DISPLAY { get; set; }
        public EmpOrgInfoModel[] items { get; set; }
    }
}
