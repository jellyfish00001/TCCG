using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IPlanBasicAService
    {
        /// <summary>
        /// 取得基本計畫資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<PlanBasicAModel> GetPlanBasicA(string PROJECT_NO);

        /// <summary>
        /// 取計畫編號
        /// </summary>
        /// <param name="planYear"></param>
        /// <param name="type"></param>
        /// <param name="OU_ID"></param>
        /// <returns></returns>
        Task<string> GenProjectNo(string planYear, string type, string OU_ID);

        /// <summary>
        /// 基本計畫資料存檔
        /// </summary>
        /// <returns></returns>
        Task<PlanBasicAModel> SavePlanBasicA(PlanBasicAModel model);

        /// <summary>
        /// 更新基本計畫資料
        /// </summary>
        /// <param name="crossAMTAList"></param>
        /// <returns></returns>
        Task EditCrossAMTA(List<PlanCrossAMTAModel> crossAMTAList);

        /// <summary>
        /// 上傳附件
        /// </summary>
        /// <param name="files"></param>
        void UploadFiles(List<ProjectAttachmentModel> files);

        /// <summary>
        /// 複製計畫
        /// </summary>
        /// <returns></returns>
        Task<object> copyProject(string PLANNO);


    }
}
