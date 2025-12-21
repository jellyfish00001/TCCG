using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Models
{
    public interface IRtnResult
    {
        public bool success { get; set; }
        public string message { get; set; }
    }
}
