using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class HiPKIModel: RtnResultModel
    {
        public HiPKIModel(bool success, string message) : base(success, message)
        {
        }

        public string tbs { get; set; }
        public string Subject { get; set; }
        public string Issuer { get; set; }
        public string SerialNumber { get; set; }
        public DateTime signTime { get; set; }
        public string cardNumber { get; set; }
        public DateTime NotBefore { get; set; }
        public DateTime NotAfter { get; set; }
    }
}
