using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDO.Models;

namespace SDO.Services
{
    public interface IRDProjectManageService
    {
        Task<List<RDProjectManageModel>> GetRDProjectManage(RDProjectManageQueryModel model);
    }
}
