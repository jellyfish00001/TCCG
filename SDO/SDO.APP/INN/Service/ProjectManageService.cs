using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using SDO.APP.INN.Models.ProjectManage;
using SDO.Base.Utils;
using SDO.Base.Utils.Models;
using SDO.Dac;
using SDO.Models;
using SDO.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace SDO.Services
{
    public class ProjectManageService : Service, IProjectManageService
    {
        private readonly IProjectManageDac dac;
        private readonly IUserData userInfo;

        public ProjectManageService(
            IProjectManageDac dac,
            IUserProfile userProfile
           )
        {
            this.dac = dac;
            this.userInfo = userProfile.GetLoginUser();

        }

        /// <summary>
        /// 取得計畫列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ProjectManageModel>> GetInnProjectManage(ProjectManageQueryModel model)
        {
            //判斷是否是重提案登入
            if (model.isProjectProposal)
            {
                model.CRT_USER = userInfo.USER_ID;
            }
            var projectList = await dac.GetProjectManage(model);

            projectList.ForEach(item =>
            {
                item.SPONSOR_ORG_UNIT = !string.IsNullOrEmpty(item.SPONSOR_ORG) && !string.IsNullOrEmpty(item.SPONSOR_UNIT)
               ? $"{item.SPONSOR_ORG} ( {item.SPONSOR_UNIT})"
               : null;
            });

            return projectList;
        }
    }
}
