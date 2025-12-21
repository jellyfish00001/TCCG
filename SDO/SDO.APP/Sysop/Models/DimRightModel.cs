using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SDO.Models
{
    public class DimRightModel : DbEditor
    {
        [Required]
        public string RIGHT_ID { get; set; }
        [Required]
        public string RIGHT_NAME { get; set; }
        public bool DEL_FLG { get; set; }
        public string[] FUNCTIONS { get; set; }
    }
}
