using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SDO.Models
{
    public class SetParamItemModel : DbEditor
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        [Display(Name = "SetParamItem_SET_ITEM", ResourceType = typeof(i18N.Label))]
        public string SET_ITEM { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [Display(Name = "SetParamItem_SET_ITEM_NAME", ResourceType = typeof(i18N.Label))]
        public string SET_ITEM_NAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SET_ITEM_DISPLAY
        {
            get
            {
                return string.Format("{0}-{1}", this.SET_ITEM, this.SET_ITEM_NAME);
            }
            private set { }
        }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "SetParamItem_MEMO", ResourceType = typeof(i18N.Label))]
        [MaxLength(500)]
        public string MEMO { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool EDITABLE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool DEL_FLG { get; set; }
    }
}
