using SDO.Dac;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SDO.Services
{
    public class ReviewListService : Service, IReviewListService
    {
        private readonly IReviewListDac dac;

        public ReviewListService(IReviewListDac dac)
        {
            this.dac = dac;
        }

        /// <summary>
        /// 查詢先期計畫資料
        /// </summary>
        /// <returns></returns>
        public async Task<List<ReviewListModel>> GetPWSReviewList(ReviewListQueryModel model)
        {
            var result = await dac.GetPWSReviewList(model);
            return result;
        }

        /// <summary>
        /// 退回先期計畫資料
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SetPWSProject(List<ReviewListModel> model)
        {
            await dac.SetPWSProject(model);
            return true;
        }
    }
}
