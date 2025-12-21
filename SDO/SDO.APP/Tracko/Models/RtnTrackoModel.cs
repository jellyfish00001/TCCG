using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class RtnTrackoModel
    {
        /// <summary>
        /// 本日到期案件-議會案件
        /// </summary>
        public List<TrackoModel> OverParliamentProjs { get; set; } = new List<TrackoModel>();

        /// <summary>
        /// 本日到期案件-專案追蹤
        /// </summary>
        public List<TrackoModel> OverTrackProjs { get; set; } = new List<TrackoModel>();

        /// <summary>
        /// 議會案件
        /// </summary>
        public List<TrackoModel> ParliamentProjs { get; set; } = new List<TrackoModel>();

        /// <summary>
        /// 專案追蹤
        /// </summary>
        public List<TrackoModel> TrackProjs { get; set; } = new List<TrackoModel>();

        /// <summary>
        /// 是否使用新版本
        /// </summary>
        public bool IsNewVersion { get; set; }
    }
}
