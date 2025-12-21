using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 配合前端功能列所需結構
    /// </summary>
    public class SetFunctionListModel
    {
        /// <summary>
        /// FUNCTION_NAME
        /// </summary>
        public string title { get; set; }
        public string functionId { get; set; }
        public string parentid { get; set; }
        public int sortId { get; set; }
        public IList<SetFunctionListModel> children { get; set; }

        public string functionUrl { get; set; }
    }
}
