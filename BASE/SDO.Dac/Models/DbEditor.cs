using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class DbEditor : IDbEditor
    {
        public string CRT_USER { get; set; }
        public string MDF_USER { get; set; }

        public int editType { get; set; }

    }
}
