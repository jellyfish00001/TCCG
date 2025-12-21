using Microsoft.AspNetCore.Http;
using SDO.APP.INN.Models.ProjectManage;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IProjectManageService
    {
        /// <summary>
        /// 抓取提案
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<ProjectManageModel>> GetInnProjectManage(ProjectManageQueryModel model);
    }
}
