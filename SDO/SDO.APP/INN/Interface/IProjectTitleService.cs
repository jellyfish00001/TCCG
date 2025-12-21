using Microsoft.AspNetCore.Http;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IProjectTitleService
    {        
        /// <summary>
        /// 取得維護專題
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        Task<MaintainInnProjectTitleModel> GetMaintainInnProjectTitle(string INN_YEAR);

        /// <summary>
        /// 儲存維護專題
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> SaveInnProjectTitle(MaintainInnProjectTitleModel model);
    }
}
