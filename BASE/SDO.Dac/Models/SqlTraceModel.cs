using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class SqlTraceModel
    {
        public string USER_ID { get; set; }
        public string USER_IP { get; set; }
        public string USER_MACHINE { get; set; }
        public string COMMANDTEXT { get; set; }
        public string PARAMETERS { get; set; }
        public string REQUEST_URL { get; set; }
    }
}
