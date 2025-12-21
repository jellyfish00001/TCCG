using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IPlanBasicADac : IDac
    {
        /// <summary>
        /// 取得基本計畫資料
        /// </summary>
        /// <param name="PLANNO"></param>
        Task<PlanBasicAModel> GetPlanBasicA(string PLANNO);

        /// <summary>
        /// 取得跨年度經費資料
        /// </summary>
        /// <param name="PLANNO"></param>
        Task<List<PlanCrossAMTAModel>> GetCrossAMTA(string PLANNO);

        /// <summary>
        /// 從維護年度取最大年度
        /// </summary>
        /// <returns></returns>
        Task<int> GetPlanYear();

        /// <summary>
        /// 基本計畫資料存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task SavePlanBasicA(PlanBasicAModel model); 

        /// <summary>
        /// 更新基本計畫資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task UpdatePlanBasicA(PlanBasicAModel model);


        /// <summary>
        /// 複製計畫
        /// </summary>
        /// <param name="model"></param>
        Task<bool> copyProject(PlanBasicAModel model);

        /// <summary>
        /// 複製計畫
        /// </summary>
        /// <param name="model"></param>
        Task<bool> copyProjectCheckPoint(PlanBasicAModel model);

        /// <summary>
        /// 跨年度經費存檔
        /// </summary>
        /// <param name="model"></param>
        Task SaveCrossAMTA(List<PlanCrossAMTAModel> models);

        /// <summary>
        /// 更新跨年度經費
        /// </summary>
        /// <param name="model"></param>
        Task UpdateCrossAMTA(List<PlanCrossAMTAModel> models);

        /// <summary>
        /// 刪除跨年度經費
        /// </summary>
        /// <param name="model"></param>
        Task DeleteCrossAMTA(List<PlanCrossAMTAModel> models);

        /// <summary>
        /// 取檢核點資料
        /// </summary>
        /// <param name="PRJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectCusCheckpointModel>> GetProjectCheckPoint(string PRJECT_NO);

        /// <summary>
        /// 刪除檢核點資料(多筆)
        /// </summary>
        /// <param name="PRJECT_NO"></param>
        Task DeleteProjectCheckPoints(string PROJECT_NO);

        /// <summary>
        /// 刪除檢核點資料(單筆)
        /// </summary>
        /// <param name="PRJECT_NO"></param>
        Task InsertProjectCheckPoint(List<ProjectCusCheckpointModel> models);

        /// <summary>
        /// 刪除檢核點資料
        /// </summary>
        /// <param name="PRJECT_NO"></param>
        Task UpdateProjectCheckPoint(List<ProjectCusCheckpointModel> models);

        /// <summary>
        /// 刪除檢核點資料
        /// </summary>
        /// <param name="PRJECT_NO"></param>
        Task DeleteProjectCheckPoint(List<ProjectCusCheckpointModel> models);

        /// <summary>
        /// 取得計劃編號流水號
        /// </summary>
        /// <param name="projectNoStart6Char">計劃編號前6碼</param>
        /// <returns></returns>
        Task<string> GetProjectNoSeq(string projectNoStart6Char);

    }


}
