using Microsoft.AspNetCore.Mvc;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface ISubmitService
    {
        /// <summary>
        /// 檢核計畫
        /// </summary>
        /// <returns></returns>
        Task<SubmitModel> CheckProjectFillSubmit([FromForm] string PROJECT_NO, [FromForm] int PLANKIND);

        /// <summary>
        /// 計畫送出
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<bool> ProjectFillSubmit([FromBody] string PROJECT_NO);
    }
}
