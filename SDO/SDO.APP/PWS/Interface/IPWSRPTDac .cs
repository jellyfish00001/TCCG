using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IPWSRPTDac : IDac
    {
        /// <summary>
        /// 取得計畫小組審查資料 (2Excal)
        /// </summary>
        /// <param name="model"></param>
        Task<List<RPTBudgeReviewModel>> GetPlanReview(PWSReportModel model);

        /// <summary>
        /// 取得計畫全部彙整表 (2Excal)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<RPTBudgeReviewModel>> GetALLORGPlan(PWSReportModel model);

        /// <summary>
        /// 取得計畫基金全部彙整表 (Excal)
        /// </summary>
        /// <param name="model"></param>
        Task<List<RPTBudgeReviewModel>> GetALLFUNDPlan(PWSReportModel model);

        /// <summary>
        /// 取重大計畫審查NO表 (Word)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<string>> GetPlanReviewNOList(PWSReportModel model);

        /// <summary>
        /// 取性別辦理評估計畫清單 Word
        /// </summary>
        /// <param name="model"></param>
        Task<List<RPTProjectReviewList>> GetPlanGenderAnalyst(PWSReportModel model);

        /// <summary>
        /// 取重大計畫審查 Word
        /// </summary>
        /// <param name="model"></param>
        Task<RPTProjectReviewList> GetPlanReviewList(PWSReportModel model);

        /// <summary>
        /// 先期審查-委託研究計畫先期審查計畫表(管考+機關)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<RPTProjectEntListModel> GetEntPlanReviewList(PWSReportModel model);

        /// <summary>
        /// 先期審查-重大施政計畫審查結果彙整表(管考)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<RPTBudgeReviewModel>> GetEntPlanMg(PWSReportModel model);

        /// <summary>
        /// 先期審查-重大施政計畫審查結果彙整表(機關)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<RPTBudgeReviewModel>> GetEntPlanOrg(PWSReportModel model);


    }


}
