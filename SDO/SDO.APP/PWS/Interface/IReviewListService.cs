using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IReviewListService
    {
        /// <summary>
        /// 查詢先期計畫資料
        /// </summary>
        /// <returns></returns>
        Task<List<ReviewListModel>> GetPWSReviewList(ReviewListQueryModel model);

        /// <summary>
        /// 退回先期計畫資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> SetPWSProject(List<ReviewListModel> model);
    }
}
