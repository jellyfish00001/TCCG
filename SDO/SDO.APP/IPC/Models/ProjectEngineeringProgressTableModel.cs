using SDO.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class ProjectEngineeringProgressTableModel : ProjectEngineeringProgressModel, IValidatableObject
    {
        /// <summary>
        /// 是否為 施工方式為"工程類"且辦理開工的實際完成日期已填寫
        /// </summary>
        public bool IsEngStartWork { get; set; }
        /// <summary>
        /// 是否辦理竣工的實際完成日期已填寫
        /// </summary>
        public bool IsCompletedWork { get; set; }
        /// <summary>
        /// 可否存檔
        /// </summary>
        public bool CanSave { get; set; }

        /// <summary>
        /// 每月辦理情形必填欄位檢核
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(EXECUTE_CONDITION))
            {
                yield return new ValidationResult("「執行情形」", new[] { "EXECUTE_CONDITION" });
            }

            if (string.IsNullOrEmpty(ASSISTANT_ITEM))
            {
                yield return new ValidationResult("「需協辦事項」", new[] { "ASSISTANT_ITEM" });
            }
        }
    }
}
