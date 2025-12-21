using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class CalendarDetailModel: CalendarModel
    {
        public string ORG_NAME { get; set; }
        public bool Editable { get; set; }
    }
}
