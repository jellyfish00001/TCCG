using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class VerifySignatureModel
    {
        public string BASE64_PAYLOAD { get; set; }
        public string SECRET { get; set; }
    }
}
