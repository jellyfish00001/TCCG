using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IProjectTitleDac : IDac
    {
        /// <summary>
        /// 取得是否開放涉及其他提案類別
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        Task<MaintainInnProjectTitleModel> GetInnProjectIsPlural(string INN_YEAR);

        /// <summary>
        /// 取得維護專題
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        Task<List<ProjectTitleModel>> GetInnProjectTitle(string INN_YEAR);

        /// <summary>
        /// 是否可編輯
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        Task<MaintainInnProjectTitleModel> GetInnProjectIsEdit(string INN_YEAR);

        /// <summary>
        /// 新增維護專題
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task InsertInnProjectTitle(ProjectTitleModel model);

        /// <summary>
        /// 刪除維護專題
        /// </summary>
        /// <param name="id"></param>
        Task DeleteInnProjectTitle(object Year);

        /// <summary>
        /// 編輯是否開放涉及其他提案類別
        /// </summary>
        /// <param name="model"></param>
        Task UpdateInnProjectIsPlural(MaintainInnProjectTitleModel model);
   

    }
}
