using Microsoft.AspNetCore.Http;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IAssignOrgService
    {
        /// <summary>
        /// 取得截止辦理時間
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        Task<AssignOrgModel> GetInnAssignOrg(string INN_YEAR);

        /// <summary>
        /// 儲存截只辦理時間
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> SaveInnAssignOrg(AssignOrgModel model);
    }
}
