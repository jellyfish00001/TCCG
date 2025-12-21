using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class EmpAgentQryModel
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string USER_ID { get; set; }

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
