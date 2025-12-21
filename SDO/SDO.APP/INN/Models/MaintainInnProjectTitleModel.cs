using SDO.Base.Utils.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class MaintainInnProjectTitleModel : DbEditor
    {
        /// <summary>
        /// 是否涉及其他提案
        /// </summary>
        public bool IS_PLURAL { get; set; }
        
        public string YEAR { get; set; }

        /// <summary>
        /// 是否可以編輯資料
        /// </summary>
        public int IS_EDIT { get; set; }

        public List<ProjectTitleModel> InnProjectTitles { get; set; }

    }
}
