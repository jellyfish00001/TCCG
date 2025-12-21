using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class HashTokenModel
    {
        public string USER_ID { get; set; }
        public DateTime EXPIRE_DATE { get; set; }
        public string IP { get; set; }
        public string HASH { get; set; }
        public bool IsAgent { get; set; }
        public string AGENT_ID { get; set; }
    }
}
