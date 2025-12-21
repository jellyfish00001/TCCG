using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Base.Utils.Attribute
{
    public class ToUtcDateTimeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //try to modify text
            try
            {
                var utcDateTime= DateTime.SpecifyKind(Convert.ToDateTime(value), DateTimeKind.Utc);
                validationContext
                .ObjectType
                .GetProperty(validationContext.MemberName)
                .SetValue(validationContext.ObjectInstance, utcDateTime, null);
            }
            catch (Exception)
            {
            }

            //return null to make sure this attribute never say iam invalid
            return null;
        }
    }
}
