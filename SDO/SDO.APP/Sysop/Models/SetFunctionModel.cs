using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SDO.Models
{
    public class SetFunctionModel : DbEditor
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string FUNCTION_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string FUNCTION_NAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FUNCTION_URL { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FUNCTION_CONTROLLER { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PARENT_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public int SORT_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool DEL_FLG { get; set; }

        #region FOR SC
        public bool IsBookMarked { get; set; }
        #endregion
    }
}
