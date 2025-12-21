using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class CertModel
    {
        public string CardNo { get; set; }
        public string IssuerDN { get; set; }
        public string SubjectID { get; set; }
        public DateTime notAfterT { get; set; }
        public DateTime notBeforeT { get; set; }
    }
}
