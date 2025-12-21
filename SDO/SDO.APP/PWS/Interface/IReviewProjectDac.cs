using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IReviewProjectDac
    {

        /// <summary>
        /// 取計畫資料
        /// </summary>
        /// <returns></returns>
        Task<ReviewProjectModel> GetPlanData(string PLANNO);

        /// <summary>
        /// 取小組意見表
        /// </summary>
        /// <returns></returns>
        Task<List<AuditTemplateModel>> GetReviewOpinions(string PLANDATETYPE);

        /// <summary>
        /// 取計畫資料
        /// </summary>
        /// <returns></returns>
        Task<string> GetPlanRelevanceNo(string PLANNO);
        /// <summary>
        /// 新增計畫審紀錄
        /// </summary>
        Task SetReviewProject(ReviewProjectModel model);

        /// <summary>
        /// 新建計畫審紀錄
        /// </summary>
        Task SetPlanReview(ReviewProjectModel model);

        /// <summary>
        /// 修改計畫審核
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        Task UpdateReviewProject(ReviewProjectModel model);

        /// <summary>
        /// 修改計畫關聯
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        Task UpdateProjectRelevance(ReviewProjectModel model);

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
        Task SendBackProject(ReviewProjectModel model);

        /// <summary>
        /// 退回小組審核
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task BackProjectReview(ReviewProjectModel model);
    }
}
