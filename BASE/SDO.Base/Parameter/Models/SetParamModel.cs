using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SDO.Models
{
    public class SetParamModel : DbEditor
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        [Display(Name = "SetParam_SET_ITEM", ResourceType = typeof(i18N.Label))]
        public string SET_ITEM { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [Display(Name = "SetParam_SET_TYPE", ResourceType = typeof(i18N.Label))]
        public string SET_TYPE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [Display(Name = "SetParam_SET_VALUE", ResourceType = typeof(i18N.Label))]
        public string SET_VALUE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "SetParam_MEMO", ResourceType = typeof(i18N.Label))]
        [MaxLength(500)]
        public string MEMO { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool DEL_FLG { get; set; }
    }
}
