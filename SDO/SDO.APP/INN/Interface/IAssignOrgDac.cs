using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IAssignOrgDac : IDac
    {
        /// <summary>
        /// 取得計畫結案資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<AssignOrgModel> GetInnAssignOrg(string INN_YEAR);


        /// <summary>
        /// 新增編輯截止時間
        /// </summary>
        /// <param name="model"></param>
        Task AddMdfInnAssignOrgCloseDate(AssignOrgModel model);

    }
}
