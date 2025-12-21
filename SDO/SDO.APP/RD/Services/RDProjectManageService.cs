using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDO.Models;
using SDO.Dac;

namespace SDO.Services
{
    public class RDProjectManageService : IRDProjectManageService
    {
        private readonly IRDProjectManageDac dac;
        public RDProjectManageService(IRDProjectManageDac dac)
        {
            this.dac = dac;
        }

        /// <summary>
        /// 取得委託研究管理清單資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<RDProjectManageModel>> GetRDProjectManage(RDProjectManageQueryModel model)
        {
            return await dac.GetRDProjectManage(model);
        }

    }
}
