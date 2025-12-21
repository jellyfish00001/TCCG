using SDO.APP.INN.Models.ProjectManage;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IProjectManageDac : IDac
    {
        Task<List<ProjectManageModel>> GetProjectManage(ProjectManageQueryModel model);
    }
}
