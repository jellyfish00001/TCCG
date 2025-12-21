using Microsoft.AspNetCore.Http;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IProjectCusFieldService
    {        
        /// <summary>
        /// 取得自訂欄位
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        Task<MaintainInnProjectCusFieldModel> GetInnProjectCusField(string INN_YEAR);

        /// <summary>
        /// 儲存自訂欄位
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task <bool> SaveInnProjectCusField(MaintainInnProjectCusFieldModel model);
    }
}
