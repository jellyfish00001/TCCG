using SDO.APP.RD.Models.Report;
using SDO.Models;
using SDO.ReportBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IRDReportDacDac
    {
        /// <summary>
        /// 參採情形總表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<ParticipatingModel>> GetParticipating(ReportQueryModel model);

        /// <summary>
        /// 委託研究計畫執行情形調查表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<PlanExecutionModel>> GetPlanExecution(ReportQueryModel model);

        /// <summary>
        /// 續列管委託研究計畫成果及運用情形調查表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<PlanExecutionSurveyModel> GetPlanExecutionSurvery(ReportQueryModel model);

        /// <summary>
        /// 本府委託研究計畫成果及運用情形調查列管表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<PlanResultModel>> GetPlanResult(ReportQueryModel model);

        /// <summary>
        /// 季委託研究計畫列管表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<SeasonReportModel>> GetSeason(ReportQueryModel model);
    }
}
