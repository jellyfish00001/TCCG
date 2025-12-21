using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IProjectCusFieldDac : IDac
    {

        /// <summary>
        /// 取得自訂欄位
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        Task<List<ProjectCusFieldModel>> GetInnProjectCusField(string INN_YEAR);

        /// <summary>
        /// 新增自訂欄位
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task InsertInnProjectCusField(ProjectCusFieldModel model);

        /// <summary>
        /// 刪除自訂欄位
        /// </summary>
        /// <param name="id"></param>
        Task DeleteInnProjectCusField(object YEAR);

        /// <summary>
        /// 取得是否可編輯
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        Task <MaintainInnProjectCusFieldModel> GetInnProjectIsEdit(string INN_YEAR);
    }
}
