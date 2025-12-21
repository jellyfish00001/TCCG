using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IProjectChapterService
    {
        #region 計畫章節
        /// <summary>
        /// 取得計畫章節表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<ProjectChapterListModel>> GetProjectChapter(ProjectChapterQueryModel model);
        #endregion
    }
}
