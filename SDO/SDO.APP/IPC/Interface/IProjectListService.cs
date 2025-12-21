using SDO.Models;
using SDO.ReportBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IProjectListService
    {
        /// <summary>
        /// 取得計畫列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<ProjectListModel>> GetProjectList(ProjectListQueryModel model);
        /// <summary>
        /// 更新計畫釘選狀態
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="PIS_SELECT"></param>
        /// <returns></returns>
        RtnResultModel AddDelFavoriateProject(string PROJECT_NO, bool PIS_SELECT);

        /// <summary>
        /// 刪除計畫
        /// </summary>
        /// <param name="projectNos"></param>
        RtnResultModel SaveProjectCanceled(List<string> projectNos);

        /// <summary>
        /// 取得計畫異動紀錄清單
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectLogListModel>> GetProjectLogList(string PROJECT_NO);
    }
}
