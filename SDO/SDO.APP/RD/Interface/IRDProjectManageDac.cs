using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDO.Models;

namespace SDO.Dac
{
    public interface IRDProjectManageDac : IDac
    {
        /// <summary>
        /// 取得委託研究管理清單資料
        /// </summary>
        /// <param name="model"></param>
        Task<List<RDProjectManageModel>> GetRDProjectManage(RDProjectManageQueryModel model);
    }
}
