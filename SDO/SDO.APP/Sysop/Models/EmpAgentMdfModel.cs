using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class EmpAgentMdfModel : DbEditor
    {
        /// <summary>
        /// 
        /// </summary>
        public Int64 SID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string USER_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string AGENT_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public DateTime AGENT_FROM { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public DateTime AGENT_TO { get; set; }
    }
}
