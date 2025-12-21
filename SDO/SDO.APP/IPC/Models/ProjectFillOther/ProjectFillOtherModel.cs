using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 其他資料Model
    /// </summary>
    public class ProjectFillOtherModel
    {
        /// <summary>
        /// 參數檔標題
        /// </summary>
        public IList<SetParamModel> SetParam { get; set; }

        /// <summary>
        /// 其他資料招標情形
        /// </summary>
        public List<ProjectBidModel> ProjectBid { get; set; }

        /// <summary>
        /// 招標情形歷程
        /// </summary>
        public List<ProjectBidDetailModel> ProjectBidDetail { get; set; }

        /// <summary>
        /// 相關活動
        /// </summary>
        public List<ProjectActivityModel> ProjectActivity { get; set; }

        /// <summary>
        /// 相關審查
        /// </summary>
        public List<ProjectReviewModel> ProjectReview { get; set; }

        /// <summary>
        /// 廠商資訊
        /// </summary>
        public List<ProjectTenderModel> ProjectTender { get; set; }
    }
}
