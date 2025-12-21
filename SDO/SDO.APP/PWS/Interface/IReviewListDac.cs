using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IReviewListDac : IDac
    {
        /// <summary>
        /// 查詢先期計畫資料
        /// </summary>
        /// <param name="model"></param>
        Task<List<ReviewListModel>> GetPWSReviewList(ReviewListQueryModel model);

        /// <summary>
        /// 退回先期計畫資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task SetPWSProject(List<ReviewListModel> model);
        
    }


}
