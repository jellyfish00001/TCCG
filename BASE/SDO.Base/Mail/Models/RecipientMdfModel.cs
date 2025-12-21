using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class RecipientMdfModel : RecipientModel
    {
        public IList<RecipientModel> RECIPIENT { get; set; }
    }
}
