using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.ReportBuilder.Models
{
    public class RtnRptModel
    {
        public byte[] Bytes { get; set; }
        public string OutputName { get; set; }
        public string Mime { get; set; }
    }
}
