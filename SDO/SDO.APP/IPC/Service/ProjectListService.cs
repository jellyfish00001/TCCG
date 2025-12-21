using SDO.Base.RPT.Enums;
using SDO.Base.Utils;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Interface;
using SDO.ReportBuilder.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class ProjectListService : Service, IProjectListService
    {
        private readonly IProjectListDac dac;
        private readonly IProjectCommonDac projectCommonDac;
        private readonly IProjectDac projectDac;

        public ProjectListService(IProjectListDac dac, IProjectCommonDac projectCommonDac, IProjectDac projectDac)
        {
            this.dac = dac;
            this.projectCommonDac = projectCommonDac;
            this.projectDac = projectDac;
        }

        /// <summary>
        /// 取得計畫列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ProjectListModel>> GetProjectList(ProjectListQueryModel model)
        {
            return await dac.GetProjectList(model);
        }

        /// <summary>
        /// 更新計畫釘選狀態
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="PIS_SELECT"></param>
        /// <returns></returns>
        public RtnResultModel AddDelFavoriateProject(string PROJECT_NO, bool PIS_SELECT)
        {
            if (PIS_SELECT)
                dac.InsertFavoriteProject(PROJECT_NO);
            else
                dac.DeleteFavoriteProject(PROJECT_NO);

            return ChangeResult(true, PIS_SELECT ? "釘選成功" : "已取消釘選");
        }

        /// <summary>
        /// 刪除計畫
        /// </summary>
        /// <param name="projectNos"></param>
        public RtnResultModel SaveProjectCanceled(List<string> projectNos)
        {
            // 檢查是否在填報周期內
            bool isInFillCycle = projectDac.IsInTheFillCycle();
            dac.BeginTransaction();
            dac.SaveProjectCanceled(projectNos);
            // 若在填報周期內刪除計畫，需移除當期工程進度、落後原因資料
            if(isInFillCycle)
            {
                projectCommonDac.DeleteLatestProjectEngProgess(projectNos);
                projectCommonDac.DeleteLatestDelayCausal(projectNos);
            }
            dac.Commit();
            return ChangeResult(true, "刪除成功");
        }

        /// <summary>
        /// 取得計畫異動紀錄清單
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<ProjectLogListModel>> GetProjectLogList(string PROJECT_NO)
        {
            return await dac.GetProjectLogList(PROJECT_NO);
        }

    }
}

