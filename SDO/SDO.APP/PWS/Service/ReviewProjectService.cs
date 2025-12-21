using SDO.Dac;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class ReviewProjectService : Service, IReviewProjectService
    {
        private readonly IReviewProjectDac dac;
        private readonly IDAMTBDac dacDAMTB;

        public ReviewProjectService(IReviewProjectDac dac, IDAMTBDac dacDAMTB)
        {
            this.dac = dac;
            this.dacDAMTB = dacDAMTB;
        }

        /// <summary>
        /// 取計審核資料
        /// </summary>
        /// <returns></returns>
        public async Task<ReviewProjectModel> GetReviewProject( string PLANNO )
        {
            // 取計畫資料
            ReviewProjectModel model = await dac.GetPlanData(PLANNO);
            // 取計畫關聯
            model.IPC_PROJECTNO = await dac.GetPlanRelevanceNo(PLANNO);
            // 取小組意見表
            model.AuditTemplateModels = await dac.GetReviewOpinions(model.PLANDATETYPE);
            //若無計畫編號 新增計畫審紀錄
            if (string.IsNullOrEmpty(model.PLANNO)) {
                model.PLANNO = PLANNO;
                await dac.SetPlanReview(model);
            }
            return model;
        }

        /// <summary>
        /// 存審核作業
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SetReviewProject(ReviewProjectModel model)
        {
            await dac.UpdateReviewProject(model);
            if (!string.IsNullOrEmpty(model.IPC_PROJECTNO)) 
            {
                await dac.UpdateProjectRelevance(model);
            }
            return true;
        }

        /// <summary>
        /// 取重大關聯資料
        /// </summary>
        /// <returns></returns>
        public async Task<List<OtherProjectModel>> GetOtherProject()
        {
            return await dac.GetOtherProject();
        }

        /// <summary>
        /// 退回計畫
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> SendBackProject(string PLANNO)
        {
            ReviewProjectModel model = new ReviewProjectModel();
            model.PLANNO = PLANNO;
            // 退回計畫
            await dac.SendBackProject(model);
            // 退回小組審核紀錄
            await dac.BackProjectReview(model);
            return new RtnResultModel(true, "已退回");
        }
    }
}
