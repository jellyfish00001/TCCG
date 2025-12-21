using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 會議列管
    /// </summary>
    public class ProjectConferenceModel : DbEditor
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public int IDENTITY_FIELD { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 會議種類
        /// </summary>
        public string CONFERENCE_GENRE { get; set; }

        /// <summary>
        /// 會議次數
        /// </summary>
        public int CONFERENCE_NUM { get; set; }

        /// <summary>
        /// 會議時間
        /// </summary>
        public DateTime? CONFERENCE_TIME { get; set; }
    }
}
