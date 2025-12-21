using SDO.Base.Utils.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class MaintainInnProjectCusFieldModel : DbEditor
    {
        
        public string YEAR { get; set; }

        /// <summary>
        /// 是否可以編輯資料
        /// </summary>
        public int IS_EDIT { get; set; }

        public List<ProjectCusFieldModel> InnProjectCusFields { get; set; }

    }
}
