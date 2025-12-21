using SDO.Base.Utils.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 實地查證情形
    /// </summary>
    public class ProjectFactFindingModel : DbEditor
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public int SEQ { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 實地查證日期
        /// </summary>
        public DateTime? FFDATE { get; set; }

        /// <summary>
        /// 完成回覆日期
        /// </summary>
        public DateTime? COMPLETEREPLYDATE { get; set; }

        /// <summary>
        /// 查證成績
        /// </summary>
        public decimal? FFSCORE { get; set; }

        /// <summary>
        /// 委員意見
        /// </summary>
        public string FFCOMMENT { get; set; }

        /// <summary>
        /// 實地查證情形填報
        /// </summary>
        public string FFREPORT { get; set; }

        /// <summary>
        /// 填報回覆日期
        /// </summary>
        public DateTime? FFREPORT_DATE { get; set; }

        /// <summary>
        /// 是否回覆
        /// </summary>
        public bool? REPLYYN { get; set; }

        /// <summary>
        /// 管考實地查證紀錄檔案(FileKind：07)
        /// </summary>
        public List<ProjectAttachmentModel> RdecFile { get; set; }
        
        /// <summary>
        /// 主辦實地查證參採資料檔案(FileKind：08)
        /// </summary>
        public List<ProjectAttachmentModel> HandFile { get; set; }

        /// <summary>
        /// 區分上傳(管考："07"、主辦："08")
        /// </summary>
        public string FileKind { get; set; }
    }
}
