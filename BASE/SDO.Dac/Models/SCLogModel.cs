using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
   public class SCLogModel
    {
        public string USR_COMP_ID { get; set; }
        public string USR_ID { get; set; }
        public string AGENT_USR_ID { get; set; }
        public string AP_ID { get; set; }
        public string FUN_ITEM_ID { get; set; }
        public string MSG_ID { get; set; }
        public string MSG_CONTENT { get; set; }
        public string MSG_DETAIL { get; set; }
        public DateTime? LOG_TIME { get; set; }
        public string LOG_TYPE { get; set; }
        public string LOG_KIND { get; set; }
        public string LOG_TYPE2 { get; set; }
        public string PRT_FLAG { get; set; }
        public string MSG_Value { get; set; }
        public string USR_IP { get; set; }
    }
}
