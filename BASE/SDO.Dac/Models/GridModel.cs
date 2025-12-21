using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac.Models
{
    public class GridModel<T> where T : class
    {
        public List<T> Data { get; set; }

        public int TotalCount { get; set; }
    }
}
