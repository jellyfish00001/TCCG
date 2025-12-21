
using System.ComponentModel.DataAnnotations;
namespace SDO.Models
{
    public class DimRoleModel
    {
        public string AP_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ROLE_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ROLE_NAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool DEL_FLG { get; set; }
    }
}