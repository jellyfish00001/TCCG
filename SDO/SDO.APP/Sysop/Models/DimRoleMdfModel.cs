using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class DimRoleMdfModel : DbEditor
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string ROLE_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string ROLE_NAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool DEL_FLG { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string[] RIGHTS { get; set; }
    }
}
