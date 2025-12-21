using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class EmpOrgMdfModel: EmpOrgModel
    {
        /// <summary>
        /// 機關代碼
        /// </summary>
        [Required]
        [Display(Name = "EmpOrg_ORG_ID", ResourceType = typeof(i18N.Label))]
        public override string ORG_ID { get; set; }

        /// <summary>
        /// 機關名稱
        /// </summary>
        [Required]
        [Display(Name = "EmpOrg_ORG_NAME", ResourceType = typeof(i18N.Label))]
        public override string ORG_NAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [Display(Name = "EmpOrg_PARENT_ID", ResourceType = typeof(i18N.Label))]
        public override string PARENT_ID { get; set; }
    }
}
