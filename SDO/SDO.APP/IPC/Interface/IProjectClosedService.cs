using Microsoft.AspNetCore.Http;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IProjectClosedService
    {
        /// <summary>
        /// 取得計畫結案資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<ProjectFillCloseModel> GetProjectFillClose(string PROJECT_NO);

        /// <summary>
        /// 儲存計畫結案資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        RtnResultModel SaveProjectFillClose(ProjectFillCloseModel model);

    }
}
