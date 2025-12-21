using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 關聯工程會標案GridModel
    /// </summary>
    public class ProjectMapPCCGridModel:DbEditor
    {
        /// <summary>
        /// 標案uid
        /// </summary>
        public string plnprj_uid { get; set; }

        /// <summary>
        /// 標案id
        /// </summary>
        public string plnprj_id { get; set; }

        /// <summary>
        /// 標案名稱
        /// </summary>
        public string plnprj_name { get; set; }

        /// <summary>
        /// 執行機關名稱
        /// </summary>
        public string execorg_name { get; set; }
    }
}
