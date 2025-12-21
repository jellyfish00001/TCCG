using Microsoft.AspNetCore.Mvc;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IReviewProjectService
    {
        /// <summary>
        /// 取計審核資料
        /// </summary>
        /// <returns></returns>
        Task<ReviewProjectModel> GetReviewProject(string PLANNO);
        /// <summary>
        /// 存審核作業
        /// </summary>
        /// <returns></returns>
        Task<bool> SetReviewProject(ReviewProjectModel model);

        /// <summary>
        /// 取重大關聯資料
        /// </summary>
        /// <returns></returns>
        Task<List<OtherProjectModel>> GetOtherProject();

        /// <summary>
        /// 退回計畫
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        Task<RtnResultModel> SendBackProject(string PLANNO);

    }
}
