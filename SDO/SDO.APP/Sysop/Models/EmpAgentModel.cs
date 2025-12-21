using System.ComponentModel.DataAnnotations;
using System;
namespace SDO.Models
{
    public class EmpAgentModel
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
        public string AGENT_NAME { get; set; }

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

        /// <summary>
        /// 
        /// </summary>
        public bool DEL_FLG { get; set; }
    }
}